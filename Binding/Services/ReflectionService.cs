using System.Reflection;

namespace Godot.Community.ControlBinding.Services;
public static class ReflectionService
{
    public static PropertyInfo GetPropertyInfo(object instance, string propertyName)
    {
        return instance.GetType().GetProperty(propertyName, 
                    System.Reflection.BindingFlags.Public |
                    System.Reflection.BindingFlags.NonPublic |
                    System.Reflection.BindingFlags.Instance |
                    System.Reflection.BindingFlags.GetField
                    );
    }
}
