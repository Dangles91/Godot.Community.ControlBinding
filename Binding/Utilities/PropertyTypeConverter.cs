using System;
using System.ComponentModel;

namespace Godot.Community.ControlBinding.Utilities;

internal static class PropertyTypeConverter
{
    public static object ConvertValue(Type fromType, Type targetType, object value)
    {
        if (value == null)
            return value;

        object convertedValue = value;
        try
        {
            if (!fromType.Equals(targetType))
            {
                var converter = TypeDescriptor.GetConverter(targetType);
                if (converter != null)
                    convertedValue = converter.ConvertTo(value, targetType);
                else
                    convertedValue = System.Convert.ChangeType(value, targetType);
            }
        }
        catch (Exception ex)
        {
            GD.PrintErr($"ControlBinding: Unable to convert value. {ex.Message}");
            convertedValue = value?.ToString();
        }

        return convertedValue;
    }
}