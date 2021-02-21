using UnityEngine;
using System.Collections;
using System;

public class PlayerView : MonoSingleton<PlayerView>
{
    [Header("Appear")]
    public float appearDuration = 1;
    public AnimationCurve appearScaleCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public float rotationMultiplicator = 360;
    public AnimationCurve rotationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Disappear")]
    public float disappearDuration = 1;
    public AnimationCurve disappearScaleCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);


    [Header("Dash")]
    public float speed = 3;
    public AnimationCurve positionCurve = AnimationCurve.EaseInOut(0,0,1,1);

    private IEnumerator _routine;
    protected override void Awake()
    {
        base.Awake();
        transform.localScale = Vector3.zero;
    }

    public static void Dash(Vector2Int from, Vector2Int to, Action<float> onProgress, Action callback = null) => Instance._Dash(from, to, onProgress, callback);
    public void _Dash(Vector2Int from, Vector2Int to, Action<float> onProgress, Action callback)
    {
        this.PlayCoroutine(ref _routine, ()=> DashAnimation(from, to, onProgress, callback));
    }

    internal static void SetAtCoordinate(Vector2Int startPosition, bool animate, Action callback)
    {
        if (animate)
            Instance._SetAtCoordinateWithAnimation(startPosition, callback);
        else
        {
            Instance._SetAtCoordinate(startPosition);
            callback?.Invoke();
        }
    }

    private void _SetAtCoordinateWithAnimation(Vector2Int startPosition, Action callback)
    {
        transform.localScale = Vector3.zero;
        this.PlayCoroutine(ref _routine, () => AppearAnimation(GridView.GetPositionFromCoordinate(startPosition), callback));
    }

    private void _SetAtCoordinate(Vector2Int startPosition)
    {
        transform.position = GridView.GetPositionFromCoordinate(startPosition);
    }

    #region Ienumerators

    public IEnumerator DashAnimation(Vector2Int from, Vector2Int to, Action<float> onProgress, Action callback)
    {
        float t = 0;
        Vector2 startPosition = GridView.GetPositionFromCoordinate(from);
        Vector2 endPosition = GridView.GetPositionFromCoordinate(to);

        float duration = (Vector2.Distance(from, to) / speed);
        do
        {
            t += Time.deltaTime / duration;
            transform.position = Vector2.Lerp(startPosition, endPosition, positionCurve.Evaluate(t));
            if (t > 1) t = 1;
            onProgress?.Invoke(t);
            yield return null;
        } while (t < 1);

        callback?.Invoke();
    }

    public IEnumerator AppearAnimation(Vector2 position, Action callback)
    {
        float t = 0;
        float aux = 1 / appearDuration;
        transform.position = position;
        do
        {
            t += Time.deltaTime * aux;
            transform.localScale = Vector3.one * appearScaleCurve.Evaluate(t);
            transform.rotation = Quaternion.Euler(0, 0, rotationCurve.Evaluate(t) * rotationMultiplicator); 
            yield return null;
        } while (t<1);
        transform.position = position;
        callback?.Invoke();
    }

    public IEnumerator DisappearAnimation()
    {
        float t = 0;
        float aux = 1 / disappearDuration;
        do
        {
            t += Time.deltaTime * aux;
            transform.localScale = Vector3.one * disappearScaleCurve.Evaluate(t);
            transform.rotation = Quaternion.Euler(0, 0, rotationCurve.Evaluate(1-t) * rotationMultiplicator);
            yield return null;
        } while (t < 1);
    }

    #endregion

    internal static void Close()
    {
        Instance.PlayCoroutine(ref Instance._routine, Instance.DisappearAnimation);
    }

}