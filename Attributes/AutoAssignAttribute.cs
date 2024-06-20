using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

public class AutoAssignAttribute : PropertyAttribute { }

public static class AutoAssignExtensions
{
    public static void AutoAssign(this UnityEngine.Object parent)
    {
        BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
        IEnumerable<FieldInfo> allFields = parent.GetType().GetFields(bindingFlags);
        IEnumerable<FieldInfo> fields = allFields.Where(f => f.GetCustomAttributes<AutoAssignAttribute>().Count() > 0);

        foreach (FieldInfo f in fields)
        {
            if (f.GetValue(parent).ToString() == "null")
            {
                Type type = f.FieldType;
                if (typeof(Component).IsAssignableFrom(type) && typeof(Component).IsAssignableFrom(parent.GetType()))
                {
                    MethodInfo creator = typeof(GameObject).GetMethod("AddComponent", 1, new Type[0]);
                    creator = creator.MakeGenericMethod(type);
                    object newObject = creator.Invoke(((Component)parent).gameObject, null);
                    f.SetValue(parent, newObject);
                }
                if (typeof(ScriptableObject).IsAssignableFrom(type))
                {
                    MethodInfo creator = typeof(ScriptableObject).GetMethod("CreateInstance", 1, new Type[0]);
                    creator = creator.MakeGenericMethod(type);
                    object newObject = creator.Invoke(null, null);
                    f.SetValue(parent, newObject);
                }
            }
        }
    }

    public static T AutoAssign<T>(this T component, Component parent, out T output) where T : Component
    {
        if (component != null) return output = component;
        if (parent.TryGetComponent(out output)) return output;
        return output = parent.gameObject.AddComponent<T>();
    }
    public static T AutoAssign<T>(this T component, Component parent) where T : Component =>
        component.AutoAssign(parent, out T output);
    public static T AutoAssign<T>(this T scriptableObject, out T output) where T : ScriptableObject
    {
        if (scriptableObject != null) return output = scriptableObject;
        return output = ScriptableObject.CreateInstance<T>();
    }
    public static T AutoAssign<T>(this T scriptableObject) where T : ScriptableObject =>
        scriptableObject.AutoAssign(out T output);
}