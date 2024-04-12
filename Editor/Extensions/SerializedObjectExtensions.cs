using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

public static class SerializedObjectExtensions
{
    public static SerializedProperty[] GetProperties(this SerializedObject serializedObject)
    {
        IEnumerable<FieldInfo> fields = serializedObject.targetObject.GetType().GetFields(SerializedPropertyExtensions.BindingFlags);
        fields = fields.Where(f => (f.IsPrivate && f.HasAttribute<SerializeField>()) || (f.IsPublic && !f.HasAttribute<NonSerializedAttribute>()));
        return fields.Select(f => serializedObject.FindProperty(f.Name)).ToArray();
    }
}