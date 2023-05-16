using System;
using System.ComponentModel;

namespace ControlBinding.Binding.Utilities
{
    public static class PropertyTypeConverter
    {
        public static object ConvertValue(Type fromType, Type targetType, object value)
        {
            object convertedValue = value;
            try
            {   
                if(!fromType.Equals(targetType))
                {                            
                    var converter = TypeDescriptor.GetConverter(targetType);
                    if(converter != null)
                        convertedValue = converter.ConvertTo(value, targetType);
                    else
                        convertedValue = System.Convert.ChangeType(value, targetType);
                }                        
            }
            catch
            {
                convertedValue = value.ToString();
            }

            return convertedValue;
        }
    }
}