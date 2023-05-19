using Godot.Community.ControlBinding.Formatters;
using Godot.Community.ControlBinding.Services;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Godot.Community.ControlBinding.Utilities;

public class BoundPropertySetter
{
    private readonly IValueFormatter _valueFormatter;
    public BoundPropertySetter(IValueFormatter valueFormatter)
    {
        _valueFormatter = valueFormatter;
    }

    public void SetBoundPropertyValue(
        Godot.Control sourceControl,
        string sourcePropertyName,
        object targetObject,
        string targetPropertyName)
    {
        setPropertyValue(sourceControl, sourcePropertyName, targetObject, targetPropertyName, _valueFormatter?.FormatTarget);
    }

    public void SetBoundControlValue(object sourceObject, string sourcePropertyName, Godot.Control targetControl, string targetPropertyName)
    {
        setPropertyValue(sourceObject, sourcePropertyName, targetControl, targetPropertyName, _valueFormatter?.FormatControl);
    }

    private void setPropertyValue(
        object sourceObject,
        string sourcePropertyName,
        object targetObject,
        string targetPropertyName,
        Func<object, object> formatter)
    {
        if (sourceObject is null && targetObject is not null)
        {            
            var propertyInfo = ReflectionService.GetPropertyInfo(targetObject, targetPropertyName);
            propertyInfo.SetValue(targetObject, null);
            return;
        }

        if(targetObject is null)
        {
            // can't set a value on a null target
            return;
        }
        
        PropertyInfo sourcePropertyInfo = ReflectionService.GetPropertyInfo(sourceObject, sourcePropertyName);
        PropertyInfo targetPropertyInfo = ReflectionService.GetPropertyInfo(targetObject, targetPropertyName);

        var sourceValue = sourcePropertyInfo.GetValue(sourceObject);
        var targetValue = targetPropertyInfo.GetValue(targetObject);

        object convertedValue = null;
        bool formatterFailed = false;

        convertedValue = sourceValue;
        if (formatter != null)
        {
            try
            {
                convertedValue = formatter(sourceValue);
            }
            catch (Exception ex)
            {
                GD.PrintErr($"DataBinding: Failed to format target. {ex.Message}. {ex.StackTrace}");
                formatterFailed = true;
            }
        }

        if ((_valueFormatter == null || _valueFormatter.FormatControl == null) ||
            formatterFailed)
        {
            try
            {
                convertedValue = PropertyTypeConverter.ConvertValue(sourcePropertyInfo.PropertyType,
                    targetPropertyInfo.PropertyType,
                    sourceValue);
            }
            catch
            {
                convertedValue = null;
            }
        }

        if (targetValue != null && targetValue.Equals(convertedValue))
            return;

        targetPropertyInfo.SetValue(targetObject, convertedValue);
    }
}