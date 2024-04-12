using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MonoBehaviour), true)]
public class MonoBehaviorEditor : ObjectEditor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
    }
}
