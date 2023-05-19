using System.Reflection;

namespace Godot.Community.ControlBinding.Services;
public static class ReflectionService
{
    public static PropertyInfo GetPropertyInfo(object instance, string propertyName)
    {
#pragma warning disable S3011 // Reflection should not be used to increase accessibility of classes, methods, or fields
        return instance.GetType().GetProperty(propertyName,
                    System.Reflection.BindingFlags.Public |
                    System.Reflection.BindingFlags.NonPublic |
                    System.Reflection.BindingFlags.Instance |
                    System.Reflection.BindingFlags.GetField
                    );
#pragma warning restore S3011 // Reflection should not be used to increase accessibility of classes, methods, or fields
    }
}
