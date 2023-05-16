using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace ControlBinding.Binding.Services
{
    public static class ReflectionService
    {
        public static PropertyInfo GetPropertyInfo(object instance, string propertyName)
        {
            return instance.GetType().GetProperty(propertyName, System.Reflection.BindingFlags.Public | 
                        System.Reflection.BindingFlags.NonPublic |
                        System.Reflection.BindingFlags.Instance);
        }
    }
}