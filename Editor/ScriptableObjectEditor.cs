using UnityEngine;
using UnityEditor;
using System.Reflection;

[CustomEditor(typeof(ScriptableObject), true)]
public class ScriptableObjectEditor : ObjectEditor
{
    public override void OnInspectorGUI()
    {
        Header(target);

        base.OnInspectorGUI();
    }

    public static void Header(object target)
    {
        string type =
            (string)target.GetType().GetField("title", BindingFlags.Public | BindingFlags.Static)?.GetValue(null) ??
            target.GetType().Name;
        string[] path = AssetDatabase.GetAssetPath((ScriptableObject)target).Split("/");
        string parentFolder = path[path.Length - 2];

        string title = StringFormatter.ToTitleCase(((ScriptableObject)target).name);

        CustomInspector.Title(title, StringFormatter.ToTitleCase(parentFolder));
    }
}
