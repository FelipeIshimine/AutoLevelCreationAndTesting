using System;
using System.Collections;
using UnityEngine;

public class CodeAnimator
{
    public delegate bool BoolDelegate();

    public AnimationType animationType = AnimationType.Simple;

    IEnumerator rutina;

    public void Stop(MonoBehaviour caller)
    {
//        Debug.Log("Animation from " + caller.name + "  Stoped");
        if (rutina != null)
        {
            caller.StopCoroutine(rutina);
        }
    }
  
    public void StartAnimacion(MonoBehaviour caller, Action<float> cod, DeltaTimeType timeType, AnimationType animType, AnimationCurve curve, float duracion = 1, float magnitud = 1)
    {
        StartAnimacion(caller, cod, timeType, animType, curve, null, duracion, magnitud);
    }

    public void StartAnimacion(MonoBehaviour caller, Action<float> cod, DeltaTimeType timeType, AnimationType animType,AnimationCurve curve, Action afterAction = null, float duracion = 1, float magnitud = 1)
    {
        if(curve == null)
            curve = new AnimationCurve(new Keyframe[2] { new Keyframe(0, 0), new Keyframe(1, 1) });

        caller.gameObject.SetActive(true);
        Stop(caller);
        switch (animType)
        {
            case AnimationType.Simple:
                rutina = AnimationSimple(cod, timeType, curve, duracion, magnitud, afterAction);
                break;
            case AnimationType.Inverse:
                rutina = AnimationInverse(cod, timeType, curve, duracion, magnitud, afterAction);
                break;
            case AnimationType.PingPong:
                rutina = AnimationPingPong(cod, timeType, curve, duracion, magnitud, afterAction);
                break;
        }
        caller.StartCoroutine(rutina);
    }

    IEnumerator AnimationSimple(Action<float> cod, DeltaTimeType timeType, AnimationCurve curve, float duracion, float magnitud, Action afterAction = null)
    {
        float c = 0;
        float t = 0;
        do
        {
            switch (timeType)
            {
                case DeltaTimeType.deltaTime:
                    yield return null;
                    break;
                case DeltaTimeType.fixedDeltaTime:
                    yield return new WaitForFixedUpdate();
                    break;
                case DeltaTimeType.unscaledDeltaTime:
                    yield return null;
                    break;
                case DeltaTimeType.fixedUnscaledDeltaTime:
                    yield return null;
                    break;
            }
            t += GetDeltaTime(timeType) / duracion;
            //            Debug.Log(curve);
            if (curve == null) Debug.Log("CURVA NULA");
            
            c = curve.Evaluate(t) * magnitud;
            if(cod != null)
                cod.Invoke(c);
          
        } while (t < 1);
        if (afterAction != null) afterAction.Invoke();
    }

    IEnumerator AnimationInverse(Action<float> cod, DeltaTimeType timeType, AnimationCurve curve, float duracion, float magnitud, Action afterAction = null)
    {
        float c = 0;
        float t = 1;
        do
        {
            t -= GetDeltaTime(timeType) / duracion;
            c = curve.Evaluate(t) * magnitud;
            cod.Invoke(c);

            switch (timeType)
            {
                case DeltaTimeType.deltaTime:
                    yield return null;
                    break;
                case DeltaTimeType.fixedDeltaTime:
                    yield return new WaitForFixedUpdate();
                    break;
                case DeltaTimeType.unscaledDeltaTime:
                    yield return null;
                    break;
                case DeltaTimeType.fixedUnscaledDeltaTime:
                    yield return null;
                    break;
            }
        } while (t > 0);
        if (afterAction != null) afterAction.Invoke();
    }

    IEnumerator AnimationPingPong(Action<float> cod, DeltaTimeType timeType, AnimationCurve curve, float duracion, float magnitud, Action afterAction = null)
    {
        //Ida
        float c = 0;
        float t = 0;
        float d = duracion / 2;
        do
        {
            t += GetDeltaTime(timeType) / d;
            c = curve.Evaluate(t) * magnitud;
            cod.Invoke(c);

            switch (timeType)
            {
                case DeltaTimeType.deltaTime:
                    yield return null;
                    break;
                case DeltaTimeType.fixedDeltaTime:
                    yield return new WaitForFixedUpdate();
                    break;
                case DeltaTimeType.unscaledDeltaTime:
                    yield return null;
                    break;
                case DeltaTimeType.fixedUnscaledDeltaTime:
                    yield return null;
                    break;
            }
        } while (t < 1);


        //Vuelta
        c = 0;
         t = 1;
        do
        {
            t -= GetDeltaTime(timeType) / d;
            c = curve.Evaluate(t);
            cod.Invoke(c);

            switch (timeType)
            {
                case DeltaTimeType.deltaTime:
                    yield return null;
                    break;
                case DeltaTimeType.fixedDeltaTime:
                    yield return new WaitForFixedUpdate();
                    break;
                case DeltaTimeType.unscaledDeltaTime:
                    yield return null;
                    break;
                case DeltaTimeType.fixedUnscaledDeltaTime:
                    yield return null;
                    break;
            }
        } while (t > 0);
        if (afterAction != null) afterAction.Invoke();
    }

    private float GetDeltaTime(DeltaTimeType timeType)
    {
        switch (timeType)
        {
            case DeltaTimeType.deltaTime:
                return Time.deltaTime;
            case DeltaTimeType.fixedDeltaTime:
                return Time.fixedDeltaTime;
            case DeltaTimeType.unscaledDeltaTime:
                return Time.unscaledDeltaTime;
            case DeltaTimeType.fixedUnscaledDeltaTime:
                return Time.fixedUnscaledDeltaTime;
            default:
                return Time.deltaTime;
        }
    }

    public void StartWaitAndExecute(MonoBehaviour caller, float waitTime, Action postAction, bool isRealTime)
    {
        Stop(caller);
        rutina = WaitAndExecute(waitTime, postAction, isRealTime);
        caller.StartCoroutine(rutina);
    }

    IEnumerator WaitAndExecute(float waitTime, Action postAction, bool isRealTime)
    {
        if (isRealTime)
            yield return new WaitForSecondsRealtime(waitTime);
        else
            yield return new WaitForSeconds(waitTime);
        postAction.Invoke();
    }


    public void StartInfiniteAnimation(MonoBehaviour caller, Action<float> action, DeltaTimeType deltaTimeType)
    {
        Stop(caller);

        switch (deltaTimeType)
        {
            case DeltaTimeType.deltaTime:
                rutina = InfiniteAnimation(action);
                break;
            case DeltaTimeType.fixedDeltaTime:
                rutina = InfiniteAnimationFixedDeltaTime(action);
                break;
            case DeltaTimeType.unscaledDeltaTime:
            case DeltaTimeType.fixedUnscaledDeltaTime:
                rutina = InfiniteAnimationUnscaled(action);
                break;
            default:
                break;
        }
        caller.StartCoroutine(rutina);
    }

    public void StartWhile(MonoBehaviour caller, Action<float> whileAction, BoolDelegate validationBlock, DeltaTimeType deltaTimeType, Action postAction)
    {
        Stop(caller);
        rutina = While(whileAction, validationBlock, deltaTimeType, postAction);
        caller.StartCoroutine(rutina);
    }

    public void StartUntil(MonoBehaviour caller, Action<float> untilAction, BoolDelegate validationBlock, DeltaTimeType deltaTimeType, Action postAction)
    {
        Stop(caller);
        rutina = Until(untilAction, validationBlock, deltaTimeType, postAction);
        caller.StartCoroutine(rutina);
    }

    IEnumerator While(Action<float> action, BoolDelegate validationBlock, DeltaTimeType deltaTimeType, Action postAction = null)
    {
        while(validationBlock.Invoke())
        {
            switch (deltaTimeType)
            {
                case DeltaTimeType.deltaTime:
                    action.Invoke(Time.deltaTime);
                    break;
                case DeltaTimeType.fixedDeltaTime:
                    action.Invoke(Time.fixedDeltaTime);
                    break;
                case DeltaTimeType.unscaledDeltaTime:
                    action.Invoke(Time.unscaledDeltaTime);
                    break;
                case DeltaTimeType.fixedUnscaledDeltaTime:
                    action.Invoke(Time.fixedUnscaledDeltaTime);
                    break;
                default:
                    action.Invoke(Time.deltaTime);
                    break;
            }
            yield return null;
        }
        postAction?.Invoke();
    }

    IEnumerator Until(Action<float> action, BoolDelegate validationBlock, DeltaTimeType deltaTimeType, Action postAction = null)
    {
        while (validationBlock.Invoke())
        {
            switch (deltaTimeType)
            {
                case DeltaTimeType.deltaTime:
                    action.Invoke(Time.deltaTime);
                    break;
                case DeltaTimeType.fixedDeltaTime:
                    action.Invoke(Time.fixedDeltaTime);
                    break;
                case DeltaTimeType.unscaledDeltaTime:
                    action.Invoke(Time.unscaledDeltaTime);
                    break;
                case DeltaTimeType.fixedUnscaledDeltaTime:
                    action.Invoke(Time.fixedUnscaledDeltaTime);
                    break;
                default:
                    action.Invoke(Time.deltaTime);
                    break;
            }
            yield return null;
        }
        postAction?.Invoke();
    }

    IEnumerator InfiniteAnimation(Action<float> action) 
    {
        while(true)
        {
            action.Invoke(Time.deltaTime);
            yield return null;
        }
    }

    IEnumerator InfiniteAnimationUnscaled(Action<float> action)
    {
        while (true)
        {
            action.Invoke(Time.unscaledDeltaTime);
            yield return null;
        }
    }

    IEnumerator InfiniteAnimationFixedDeltaTime(Action<float> action)
    {
        while (true)
        {
            action.Invoke(Time.fixedDeltaTime);
            yield return new WaitForFixedUpdate();
        }
    }

}

public enum AnimationType { Simple, Inverse, PingPong }
public enum DeltaTimeType { deltaTime, fixedDeltaTime, unscaledDeltaTime, fixedUnscaledDeltaTime }

[System.Serializable]
public class CodeAnimatorCurve
{
    [SerializeField]
    private AnimationCurve curve = AnimationCurve.Linear(0,0,1,1);
    public AnimationCurve Curve
    {
        get { return curve; }
        set { curve = value; }
    }

    [SerializeField]
    private float time = .25f;
    public float Time
    {
        get { return time; }
        set { time = value; }
    }

    public CodeAnimatorCurve()
    {
        curve = AnimationCurve.Linear(0, 0, 1, 1);
        time = .25f;
    }
}
