using System.IO;
using UnityEditor;

public static class AssetManager
{
    public static void CreateDirectory(string directory)
    {
        string folder = "";
        string path = "";
        foreach (char c in directory)
        {
            if (c != '/' && c != '\\')
            {
                folder += c;
            }
            else
            {
                if (!Directory.Exists($"{path}/{folder}")) AssetDatabase.CreateFolder(path, folder);
                path += (path == "") ? $"{folder}" : $"/{folder}";
                folder = "";
            }
        }
        if (!Directory.Exists($"{path}/{folder}")) AssetDatabase.CreateFolder(path, folder);
    }
}

public static class AssetPaths
{
    public const string master = "Assets";

    public const string scenes = master + "/Scenes";
    public const string prefabs = master + "/Prefabs";
    public const string animations = master + "/Animations";
    public const string scriptableObjects = master + "/ScriptableObjects";
}

public static class CreateAssetPaths
{
    public const string variables = "Variable/";
}
