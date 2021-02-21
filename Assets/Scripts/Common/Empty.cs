using System;
using UnityEngine;
using System.Collections;

public class Empty : MonoBehaviour
{
    internal void WaitAndExecute(float waitTime, Action postAction)
    {
        StartCoroutine(WaitAndExecuteRutine(waitTime, postAction));
    }

    IEnumerator WaitAndExecuteRutine(float waitTime, Action callback)
    {
        yield return new WaitForSecondsRealtime(waitTime);
        callback.Invoke();
    }

}