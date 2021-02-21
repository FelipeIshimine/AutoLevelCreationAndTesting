using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class GlobalUpdate : MonoBehaviour
{
    private static bool initialized = false;

    private static Action globalUpdateEvent;
    public static Action OnUpdateEvent
    {
        get 
        {
            Initialize();
            return globalUpdateEvent; 
        }
        set { globalUpdateEvent = value; }
    }

    private static Action globalFixedUpdateEvent;
    public static Action OnFixedUpdateEvent
    {
        get { 
            Initialize();
            return globalFixedUpdateEvent; }
        set { globalFixedUpdateEvent = value; }
    }

    private static Action globalLateUpdateEvent;
    public static Action OnLateUpdateEvent
    {
        get { 
            Initialize();
            return globalLateUpdateEvent; }
        set { globalLateUpdateEvent = value; }
    }

    private static GlobalUpdate instance = null;

    [RuntimeInitializeOnLoadMethod]
    private static void Initialize()
    {
        if (initialized) return;
        initialized = true;
        instance = new GameObject().AddComponent<GlobalUpdate>();
        DontDestroyOnLoad(instance);
        instance.gameObject.name = "Global Update";

        globalUpdateEvent = null;
        globalFixedUpdateEvent = null;
        globalLateUpdateEvent = null;
    }

    public static void _StartCoroutine(IEnumerator rutine)
    {
        instance.StartCoroutine(rutine);
    }


    private void Update()
    {
        OnUpdateEvent?.Invoke();
    }

    private void LateUpdate()
    {
        OnLateUpdateEvent?.Invoke();
    }

    private void FixedUpdate()
    {
        OnFixedUpdateEvent?.Invoke();
    }

    internal static void _StartCoroutine(object transitionOut)
    {
        throw new NotImplementedException();
    }
}
