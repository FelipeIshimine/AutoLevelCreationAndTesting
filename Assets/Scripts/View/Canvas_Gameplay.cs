using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Canvas_Gameplay : MonoBehaviour
{
    public static event Action OnRestart;
    public static event Action<int> OnLevelRequest;
    public static event Action OnSkip;

    [Button]
    public void SkipRequest()
    {
        Debug.Log("SKIP");
        OnSkip?.Invoke();
    }
    [Button]
    public void RestartRequest()
    {
        Debug.Log("RESTART");
        OnRestart?.Invoke();
    }

    [Button]
    public void SelectLevel(int value)
    {
        OnLevelRequest?.Invoke(value);
    }

}
