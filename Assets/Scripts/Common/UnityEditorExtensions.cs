using UnityEditor;
using UnityEngine;

public static class UnityEditorExtensions
{
#if UNITY_EDITOR
    /// <summary>
    /// Empieza automaticamente desde Assets
    /// </summary>
    /// <param name="folders"></param>
    public static string CreateFoldersRecursive(params string[] folders)
    {
        string currentPath = "Assets/";
        for (int i = 0; i < folders.Length; i++)
        {
            Debug.Log(currentPath);
            string parentFolder = currentPath.Remove(currentPath.Length - 1, 1);
            string subfolder = folders[i];
            if(!AssetDatabase.IsValidFolder(currentPath + subfolder))
                AssetDatabase.CreateFolder(parentFolder, subfolder);
            currentPath += $"{folders[i]}/";
        }
        AssetDatabase.Refresh();
        return currentPath;
    }
    
    public static T AssetFromGUID<T>(string assetGUID) where T : Object
    {
        string path = AssetDatabase.GUIDToAssetPath(assetGUID);
        return  AssetDatabase.LoadAssetAtPath<T>(path);
    }
#endif
}