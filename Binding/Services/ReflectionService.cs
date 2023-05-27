using System.Collections.Generic;
using System.Reflection;

namespace Godot.Community.ControlBinding.Services;
public static class ReflectionService
{
    private static readonly Dictionary<string, PropertyInfo> _propertyInfoCache = new();

    public static PropertyInfo GetPropertyInfo(object instance, string propertyName)
    {
        if (instance == null)
            return null;

        string cacheKey = $"{instance.GetType().FullName}.{propertyName}";

        if (!_propertyInfoCache.ContainsKey(cacheKey))
        {
            var pInfo = instance.GetType().GetProperty(propertyName,
                    System.Reflection.BindingFlags.Public |
                    System.Reflection.BindingFlags.NonPublic |
                    System.Reflection.BindingFlags.Instance |
                    System.Reflection.BindingFlags.GetField
                    );

            if (pInfo == null)
                return null;
            _propertyInfoCache.Add(cacheKey, pInfo);
        }

#pragma warning disable S3011 // Reflection should not be used to increase accessibility of classes, methods, or fields
        return _propertyInfoCache[cacheKey];
#pragma warning restore S3011 // Reflection should not be used to increase accessibility of classes, methods, or fields
    }
}
