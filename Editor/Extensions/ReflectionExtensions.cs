using System;
using System.Linq;
using System.Reflection;

public static class ReflectionExtensions
{
    public static bool HasAttribute<T>(this MemberInfo element) where T : Attribute => element.GetCustomAttribute<T>() != null;
    public static FieldInfo[] GetFieldsWithAttribute<T>(this Type type, BindingFlags bindingFlags = SerializedPropertyExtensions.BindingFlags) where T : Attribute =>
        type.GetFields(bindingFlags).Where(f => f.HasAttribute<T>()).ToArray();
    public static PropertyInfo[] GetPropertiesWithAttribute<T>(this Type type, BindingFlags bindingFlags = SerializedPropertyExtensions.BindingFlags) where T : Attribute =>
        type.GetProperties(bindingFlags).Where(p => p.HasAttribute<T>()).ToArray();
    public static MethodInfo[] GetMethodsWithAttribute<T>(this Type type, BindingFlags bindingFlags = SerializedPropertyExtensions.BindingFlags) where T : Attribute =>
        type.GetMethods(bindingFlags).Where(m => m.HasAttribute<T>()).ToArray();

    public static bool TryGetField(this Type type, string name, out FieldInfo result) =>
        (result = type.GetField(name)) != null;
    public static bool TryGetProperty(this Type type, string name, out PropertyInfo result) =>
        (result = type.GetProperty(name)) != null;
    public static bool TryGetMethod(this Type type, string name, out MethodInfo result) =>
        (result = type.GetMethod(name)) != null;

    public static object Default(this Type type)
    {
        if (type.IsValueType)
            return Activator.CreateInstance(type);

        return null;
    }
}