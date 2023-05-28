using Godot.Community.ControlBinding.Formatters;
using Godot.Community.ControlBinding.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Godot.Community.ControlBinding.Utilities;

public class BoundPropertySetter
{
    private readonly IValueFormatter _valueFormatter;
    public readonly List<Func<object, string>> Validators;
    public BoundPropertySetter(IValueFormatter valueFormatter, List<Func<object, string>> validators = null)
    {
        _valueFormatter = valueFormatter;
        Validators = validators;
    }

    private enum SetDirection
    {
        ToProperty,
        ToControl
    }

    public void SetBoundPropertyValue(
        Godot.Control sourceControl,
        string sourcePropertyName,
        object targetObject,
        string targetPropertyName)
    {
        setPropertyValue(sourceControl, sourcePropertyName, targetObject, targetPropertyName, _valueFormatter?.FormatTarget, SetDirection.ToProperty);
    }

    public void SetBoundControlValue(object sourceObject, string sourcePropertyName, Godot.Control targetControl, string targetPropertyName)
    {
        setPropertyValue(sourceObject, sourcePropertyName, targetControl, targetPropertyName, _valueFormatter?.FormatControl, SetDirection.ToControl);
    }

    private void setPropertyValue(
        object sourceObject,
        string sourcePropertyName,
        object targetObject,
        string targetPropertyName,
        Func<object, object, object> formatter,
        SetDirection direction)
    {
        if (sourceObject is null && targetObject is not null)
        {
            var propertyInfo = ReflectionService.GetPropertyInfo(targetObject, targetPropertyName);
            propertyInfo.SetValue(targetObject, null);
            return;
        }

        if (targetObject is null)
        {
            // can't set a value on a null target
            return;
        }

        PropertyInfo sourcePropertyInfo = ReflectionService.GetPropertyInfo(sourceObject, sourcePropertyName);
        PropertyInfo targetPropertyInfo = ReflectionService.GetPropertyInfo(targetObject, targetPropertyName);

        var sourceValue = sourcePropertyInfo.GetValue(sourceObject);
        var targetValue = targetPropertyInfo.GetValue(targetObject);

        object convertedValue = sourceValue;
        bool formatterFailed = false;

        if(direction == SetDirection.ToProperty && Validators?.Any() == true)
        {
            foreach(var validator in Validators)
            {
                var result = validator(sourceValue);
                if(!string.IsNullOrEmpty(result))
                {
                    throw new ValidationException(result);
                }
            }
        }

        if (formatter != null)
        {
            try
            {
                var previousValue = targetValue;
                convertedValue = formatter(sourceValue, targetValue);
            }
            catch(ValidationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                GD.PrintErr($"DataBinding: Failed to format target. {ex.Message}");
                formatterFailed = true;
            }
        }

        if (_valueFormatter == null || _valueFormatter.FormatControl == null ||
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

        if (targetValue?.Equals(convertedValue) == true)
            return;

        if (targetValue == convertedValue)
            return;

        targetPropertyInfo.SetValue(targetObject, convertedValue);
    }
}