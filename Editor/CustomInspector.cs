using System;
using System.Reflection;
using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Collections.Generic;
using Object = UnityEngine.Object;

public class CustomInspector : UnityEditor.Editor
{
    public T GetField<T>(params string[] path)
    {
        object targetObject = target;
        foreach (string f in path)
        {
            FieldInfo field = targetObject.GetType().GetField(f, SerializedPropertyExtensions.BindingFlags);
            targetObject = field.GetValue(targetObject);
        }
        return SerializedPropertyExtensions.Convert<T>(targetObject);
    }

    public void SetField(object value, params string[] path)
    {
        object targetObject = target;
        FieldInfo field = targetObject.GetType().GetField(path[0], SerializedPropertyExtensions.BindingFlags);
        for (int i = 1; i > path.Length - 1; i++)
        {
            targetObject = field.GetValue(targetObject);
            field = targetObject.GetType().GetField(path[i], SerializedPropertyExtensions.BindingFlags);
        }
        if (field.FieldType.IsAssignableFrom(targetObject.GetType()))
        { field.SetValue(targetObject, value); return; }
        throw new InvalidCastException($"Cannot convert from type \"{targetObject.GetType()}\" to type \"{field.FieldType}\"");
    }

    private static GUIStyle b_TitleStyle;
    public static GUIStyle TitleStyle => b_TitleStyle ??= new GUIStyle(GUI.skin.label)
    {
        fontSize = 20,
        fontStyle = FontStyle.Bold,
        alignment = TextAnchor.MiddleCenter
    };
    
    private static GUIStyle b_BoldLabelStyle;
    public static GUIStyle BoldLabelStyle => b_BoldLabelStyle ??= new GUIStyle(GUI.skin.label)
    {
        fontStyle = FontStyle.Bold
    };

    private static GUIStyle b_CenterStyle;
    public static GUIStyle CenterStyle => b_CenterStyle ??= new GUIStyle(GUI.skin.label)
    {
        alignment = TextAnchor.MiddleCenter
    };

    public static void Title(string title, string subTitle = null)
    {
        GUILayout.Space(10f);

        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();

        GUILayout.Label(title, TitleStyle);

        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();

        if (subTitle != null)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            GUILayout.Label(subTitle);

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
        }

        GUILayout.Space(20f);
    }

    public static float GetRelativeWidth(float percentage)
    {
        return (Screen.width - 26) * percentage * .01f;
    }

    public PropertyInfo[] GetDisplayProperties()
    {
        IEnumerable<PropertyInfo> properties = target.GetType().GetProperties(SerializedPropertyExtensions.BindingFlags);

        if (target.GetType().HasAttribute<DisplayPropertiesAttribute>())
            properties = properties.Where(p => !p.HasAttribute<HideAttribute>());
        else
        {
            if (EditorApplication.isPlaying)
                properties = properties.Where(p => p.HasAttribute<DisplayAttribute>() || p.HasAttribute<DisplayPlayModeAttribute>());
            else
                properties = properties.Where(p => p.HasAttribute<DisplayAttribute>());
        }

        return properties.ToArray();
    }

    public Dictionary<string, object[]> Methods = new();

    public void ShowButtons()
    {
        IEnumerable<MethodInfo> methods = target.GetType().GetMethods(SerializedPropertyExtensions.BindingFlags);
        methods = methods.Where(m => m.HasAttribute<ButtonAttribute>());
        foreach (MethodInfo method in methods)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel(method.Name.ToTitleCase());
            if (method.GetParameters().Length == 0)
            {
                if (GUILayout.Button("Invoke")) method.Invoke(target, null);
            }
            if (method.GetParameters().Length == 1)
            {
                Type type = method.GetParameters()[0].ParameterType;
                if (IsValidParameterType(type))
                {
                    if (!Methods.ContainsKey(method.Name)) Methods.Add(method.Name, new object[] { type.Default() });
                    Methods[method.Name][0] = ValueField(Methods[method.Name][0], type);
                    if (GUILayout.Button("Invoke")) method.Invoke(target, Methods[method.Name]);
                }
                else
                {
                    EditorGUILayout.LabelField("Cannot display this method");
                }
            }
            EditorGUILayout.EndHorizontal();
        }
    }

    public static object ValueField(object value, Type type)
    {
        if (type == typeof(int)) return EditorGUILayout.IntField((int)value);
        if (type == typeof(float)) return EditorGUILayout.FloatField((float)value);
        if (type == typeof(string)) return EditorGUILayout.TextField((string)value);
        if (type == typeof(bool)) return EditorGUILayout.Toggle((bool)value);
        if (typeof(Object).IsAssignableFrom(type)) return EditorGUILayout.ObjectField((Object)value, type, true);
        return null;
    }

    public static bool IsValidParameterType(Type type)
    {
        Type[] validTypes = new Type[]
        {
            typeof(int),
            typeof(float),
            typeof(string),
            typeof(bool)
        };
        if (validTypes.Contains(type)) return true;
        if (typeof(Object).IsAssignableFrom(type)) return true;
        return false;
    }
}
