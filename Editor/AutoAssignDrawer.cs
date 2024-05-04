using UnityEngine;
using UnityEditor;
using System;
using System.Reflection;
using System.Text.RegularExpressions;


namespace ArchitectureLibrary.Editor
{
    [CustomPropertyDrawer(typeof(AutoAssignAttribute))]
    public class AutoAssignDrawer : PropertyDrawer
    {
        private bool ErrorMessageDisplayed = false;
        public const float buttonWidth = 20;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            Type type = property.GetPropertyType();
            MethodInfo GUIMethod;

            if (typeof(Component).IsAssignableFrom(type))
            {
                GUIMethod = GetType().GetMethod(nameof(ComponentGUI));
                GUIMethod = GUIMethod.MakeGenericMethod(type);
                GUIMethod.Invoke(null, new object[3] { position, property, label });
            }
            else if (typeof(ScriptableObject).IsAssignableFrom(type))
            {
                GUIMethod = GetType().GetMethod(nameof(ScriptableObjectGUI));
                GUIMethod = GUIMethod.MakeGenericMethod(type);
                GUIMethod.Invoke(null, new object[3] { position, property, label });
            }
            else
            {
                EditorGUI.PropertyField(position, property, label);
                if (!ErrorMessageDisplayed)
                {
                    Debug.LogWarning($"AutoAssign attribute is not valid for serialized property, \"{property.name}\". It is only valid for object reference values.");
                    ErrorMessageDisplayed = true;
                }
            }
        }

        public static void ComponentGUI<T>(Rect position, SerializedProperty property, GUIContent label) where T : Component
        {
            SerializedObject parent = property.serializedObject;
            label = EditorGUI.BeginProperty(position, label, property);
            position = EditorGUI.PrefixLabel(position, label);

            EditorGUI.BeginChangeCheck();

            if (property.objectReferenceValue == null && parent != null && typeof(Component).IsAssignableFrom(parent.targetObject.GetType()))
            {
                GameObject gameObject = ((Component)parent.targetObject).gameObject;

                if (gameObject.TryGetComponent(out T component))
                    property.objectReferenceValue = component;
                else if (gameObject.GetComponentInParent<T>() != null)
                    property.objectReferenceValue = gameObject.GetComponentInParent<T>();
                else if (gameObject.GetComponentInChildren<T>() != null)
                    property.objectReferenceValue = gameObject.GetComponentInChildren<T>();
                else
                {
                    Rect fieldPosition = new(position);
                    fieldPosition.width -= buttonWidth + 2;

                    Rect buttonPosition = new(position);
                    buttonPosition.x += fieldPosition.width + 2;
                    buttonPosition.width = buttonWidth;

                    property.objectReferenceValue =
                        EditorGUI.ObjectField(fieldPosition, property.objectReferenceValue, typeof(T), true);
                    if (GUI.Button(buttonPosition, "+")) property.objectReferenceValue = gameObject.AddComponent<T>();
                }
            }
            else
            {
                property.objectReferenceValue =
                    EditorGUI.ObjectField(position, property.objectReferenceValue, typeof(T), true);
            }

            EditorGUI.EndProperty();
        }

        public static void ScriptableObjectGUI<T>(Rect position, SerializedProperty property, GUIContent label) where T : ScriptableObject
        {
            SerializedObject parent = property.serializedObject;
            label = EditorGUI.BeginProperty(position, label, property);
            position = EditorGUI.PrefixLabel(position, label);

            EditorGUI.BeginChangeCheck();

            if (property.objectReferenceValue == null)
            {
                Rect fieldPosition = new(position);
                fieldPosition.width -= buttonWidth + 2;

                Rect buttonPosition = new(position);
                buttonPosition.x += fieldPosition.width + 2;
                buttonPosition.width = buttonWidth;

                property.objectReferenceValue =
                    EditorGUI.ObjectField(fieldPosition, property.objectReferenceValue, typeof(T), true);

                if (GUI.Button(buttonPosition, "+"))
                {
                    string directory = AssetPaths.scriptableObjects;
                    string name = StringFormatter.CapitalizeFirst(property.name);
                    name = Regex.Replace(name, "Data$", "");
                    if (parent != null && typeof(Component).IsAssignableFrom(parent.targetObject.GetType()))
                    {
                        directory += "/" + ((Component)parent.targetObject).name;
                        name += parent.targetObject.GetType().Name;
                    }
                    if (name == "") name = "NewObject";

                    property.objectReferenceValue = ScriptableObjectFactory.Create<T>(directory, name);
                }
            }
            else
            {
                property.objectReferenceValue =
                    EditorGUI.ObjectField(position, property.objectReferenceValue, typeof(T), false);
            }

            EditorGUI.EndProperty();
        }
    }
}