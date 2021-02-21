using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

[CreateAssetMenu(menuName = "Managers/Time")]
public class TimeManager : RuntimeScriptableSingleton<TimeManager>
{
    public CodeAnimatorCurve curveIn;
    public CodeAnimatorCurve curveOut;
    public CodeAnimatorCurve resetCurve;
    public float defaultSlowMotionDuration = .1f;
    IEnumerator rutine;

    private static bool inTransition = false;
    private static bool canBeOverrided = false;

    private static bool inPause = false;
    private static float beforePauseValue = 1;

    public float longHitStopDuration = .2f;
    public float shortHitStopDuration = .1f;

    private Empty monoProxy;
    public Empty MonoProxy
    {
        get
        {
            if (monoProxy == null)
            {
                monoProxy = new GameObject().AddComponent<Empty>();
                monoProxy.gameObject.name = this + "_TimeManagerProxy";
            }
            return monoProxy;
        }
        set { monoProxy = value; }
    }

    float defaulFixedDeltaTime;

    [ShowInInspector, Sirenix.OdinInspector.ReadOnly] bool _InTransition => inTransition;
    [ShowInInspector, Sirenix.OdinInspector.ReadOnly] float TimeScale => Time.timeScale;
    [ShowInInspector, Sirenix.OdinInspector.ReadOnly] float _BaseTimeScale => BaseTimeScale;

 
    private float shiftBackSmooth = .2f;

    public float slowMotionAmmount = .2f;

    private static float baseTimeScale = 1;
    public static float BaseTimeScale
    {
        get { return baseTimeScale; }
        set
        {
            baseTimeScale = value;

            if (!inTransition)
                Instance.StartTimeShiftBack();
        }
    }

    protected void OnEnable()
    {
        defaulFixedDeltaTime = Time.fixedDeltaTime;
    }


    #region Static

    public static void _ResetAll()
    {
        _SetTimeScale(1);
        BaseTimeScale = 1;
        inTransition = false;
    }

    public static void _Accelerate_BaseTime(float percentaje)
    {
        BaseTimeScale *= percentaje;
    }

    public static void _Deaccelerate_BaseTime(float percentaje)
    {
        BaseTimeScale -= BaseTimeScale * percentaje;
    }

    public static void _ModifyFlat_BaseTime(float ammount)
    {
        BaseTimeScale += ammount;
    }

    public static void _StartDefaultSlowMotion()
    {
        _StartTimeShift(Instance.slowMotionAmmount, Instance.defaultSlowMotionDuration, true, true);
    }

    public static void _StartTimeShift(float target, float duration, bool canBeOverrided, bool useSmooth)
    {
        TimeManager.canBeOverrided = canBeOverrided;
        Instance.StartTimeShift(target, duration, useSmooth);
    }

    public static void _StartTimeShiftBack()
    {
        if (inTransition && !canBeOverrided) return;
        Instance.StartTimeShiftBack();
    }

    private static void _SetTimeScale(float nTimeScale)
    {
        Time.timeScale = nTimeScale;
        Time.fixedDeltaTime = Instance.defaulFixedDeltaTime * nTimeScale;
    }

    #endregion

    public enum HitLength { Short, Long}
    public static void HitStop(HitLength length)
    {
        Instance._HitStop(length);
    }

    private void _HitStop(HitLength length)
    {
        _StartTimeShift(.05f, (length == HitLength.Long) ? longHitStopDuration : shortHitStopDuration, true, false);
    }

    IEnumerator HitStopRutine(float duration)
    {
        float oldTimeScale = BaseTimeScale;
        _SetTimeScale(0);
        yield return new WaitForSecondsRealtime(duration);
        _SetTimeScale(oldTimeScale);
    }


    public static void Pause()
    {
        Instance._Pause();
    }

    [Button]
    private void _Pause()
    {
        Debug.LogWarning("Pause");
        if (inPause)
            return;
        inPause = true;
        beforePauseValue = Time.timeScale;
        Time.timeScale = 0;
    }

    [Button]
    public void _Resume()
    {
        Debug.LogWarning("Resume");

        if (!inPause)
            return;

        inPause = false;
        Time.timeScale = beforePauseValue;
    }

    public static void Resume()
    {
        Instance._Resume();
    }

    public void StartTimeShiftBack()
    {
        if (inTransition && !canBeOverrided)
            Debug.LogError("Ya hay una transicion activa");

        if (rutine != null) MonoProxy.StopCoroutine(rutine);
        rutine = TimeShiftTo(1, curveOut, false);
        MonoProxy.StartCoroutine(rutine);
    }

    [Button]
    public void StartTimeShift(float target, float duration, bool useSmooth)
    {
        if (rutine != null) MonoProxy.StopCoroutine(rutine);
        rutine = StartTemporalShiftRutine(target, duration, useSmooth);
        MonoProxy.StartCoroutine(rutine);
    }

    IEnumerator StartTemporalShiftRutine(float targetTime, float duration, bool useSmooth)
    {
        yield return TimeShiftTo(targetTime, curveIn, false);
        yield return new WaitForSecondsRealtime(duration);
        if(useSmooth)
            yield return TimeShiftBack(false);
        else
            yield return TimeShiftTo(1, curveOut, false);
    }

    IEnumerator TimeShiftTo(float targetTime, CodeAnimatorCurve curve, bool resetModifyFlag)
    {
        inTransition = true;
        float startTime = Time.timeScale;
        float x = 0;
        yield return null;
        do
        {
            if (inPause)
            {
                yield return null;
                continue;
            }

            x += Time.deltaTime / curve.Time;
            _SetTimeScale(Mathf.Lerp(startTime, targetTime, curve.Curve.Evaluate(x)));

            yield return null;
        } while (x < 1);
        if(resetModifyFlag) inTransition = false;
    }

    IEnumerator TimeShiftBack(bool resetModifyFlag)
    {
        inTransition = true;
        float vel = 0;
        do
        {
            if (inPause)
            {
                yield return null;
                continue;
            }
            _SetTimeScale(Mathf.SmoothDamp(Time.timeScale, BaseTimeScale, ref vel, shiftBackSmooth));
            yield return null;
        } while (Mathf.Abs(Time.timeScale - BaseTimeScale) > .005f);
        Time.timeScale = BaseTimeScale;
       if(resetModifyFlag) inTransition = false;
    }

}

public struct TimeShift
{
    public float TimeScale;
    public float Duration;

    public TimeShift(float targetTime, float duration)
    {
        TimeScale = targetTime;
        Duration = duration;
    }
}