using System;
using UnityEngine;

public class MonoSingleton<T> : MonoBehaviour where T : MonoSingleton<T> 
{
    private static T instance;
    public static T Instance
    {
        get => instance;
        set => instance = value;
    }

    protected virtual void Awake()
    {
        if(instance != null)
        {
            Destroy(gameObject);
            throw new Exception($"Mono Singleton Instance Error {this}");
        }
        instance = this as T;
    }

}