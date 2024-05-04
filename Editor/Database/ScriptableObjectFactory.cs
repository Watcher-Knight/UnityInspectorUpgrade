using UnityEngine;
using UnityEditor;

namespace ArchitectureLibrary.Editor
{
    public class ScriptableObjectFactory
    {
        public static T Create<T>(string directory, string name) where T : ScriptableObject
        {
            T so = ScriptableObject.CreateInstance<T>();

            AssetManager.CreateDirectory(directory);

            string path = $"{directory}/{name}.asset";
            path = AssetDatabase.GenerateUniqueAssetPath(path);

            AssetDatabase.CreateAsset(so, path);

            EditorGUIUtility.PingObject(so);

            return so;
        }
    }
}