using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ControlBinding.Binding.Services;
using ControlBinding.Binding.Utilities;
using Godot;

namespace ControlBinding.Binding
{    
    public class BoundPropertySetter
    {
        private IValueFormatter _valueFormatter;
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
            Func<object,object> formatter)
        {
            var sourcePropertyInfo = ReflectionService.GetPropertyInfo(sourceObject, sourcePropertyName);
            var targetPropertyInfo = ReflectionService.GetPropertyInfo(targetObject, targetPropertyName);

            if (sourcePropertyInfo is null || targetPropertyInfo is null)
                return;

            var sourceValue = sourcePropertyInfo.GetValue(sourceObject);
            var targetValue = targetPropertyInfo.GetValue(targetObject);

            object convertedValue = null;
            bool formatterFailed = false;

            if (targetObject != null)
            {
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
                    convertedValue = PropertyTypeConverter.ConvertValue(sourcePropertyInfo.PropertyType,
                        targetPropertyInfo.PropertyType,
                        sourceValue);
                }
            }

            if (targetValue != null && targetValue.Equals(convertedValue))
                return;

            targetPropertyInfo.SetValue(targetObject, convertedValue);
        }
    }
}