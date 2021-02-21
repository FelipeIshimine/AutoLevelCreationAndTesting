using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
#endif

/// <summary>
/// Singleton que sea auto instancia e inicializa dentro de la carpeta Resources
/// </summary>
/// <typeparam name="T">Referencia circular a la propia clase de la que se quiere hacer Singleton</typeparam>
public abstract class RuntimeScriptableSingleton<T> : BaseRuntimeScriptableSingleton where T : RuntimeScriptableSingleton<T>
{
    private static T _instance;
    public static T Instance
    {
        get
        {
            if (_instance != null) return _instance;
            try
            {
                _instance = Resources.LoadAll<T>("")[0];
            }
            catch (System.Exception error)
            {
                Debug.Log(error);
#if UNITY_EDITOR
                Debug.Log($"<color=orange>|||</color> RuntimeScriptableSingleton<{typeof(T)}> Searching in resources");
                _instance = CreateInstance<T>();
                AssetDatabase.CreateAsset(_instance, ResourcesPath + typeof(T).Name + ".asset");
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
#endif
            }
            _instance.InitializeSingleton();
            return _instance;
        }
        set
        {
            _instance = value;
            Debug.Log(" <Color=green> SCRIPTABLE_SINGLETON Initialized: </color> <Color=blue> " + _instance + "</color> ");
        }
    }

    public static string ResourcesPath => "Assets/Resources/";
    public virtual string FileName =>  typeof(T).Name;
    public virtual string FilePath => ResourcesPath + FileName + ".asset";

    public T Myself => this as T;

    public override void InitializeSingleton()
    {
        if (_instance != null && _instance != this)
            throw new Exception($"Singleton error {this.GetType()}");
        _instance = this as T;
    }

}

public abstract class BaseRuntimeScriptableSingleton : ScriptableObject
{
        
    #if UNITY_EDITOR
            [MenuItem("Window/Ishimine/Update/RuntimeScriptableSingleton")]
            public static void SelectMe() => Client.Add("https://github.com/FelipeIshimine/RuntimeScriptableSingleton.git");
    #endif
    public abstract void InitializeSingleton();
}