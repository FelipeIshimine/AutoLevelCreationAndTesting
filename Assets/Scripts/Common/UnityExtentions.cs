using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

public static class UnityExtentions
{
    public const float GoldenRatio = 1.61803398875f;

    public static T Last<T>(this List<T> source) => source[source.Count - 1];
    public static T First<T>(this List<T> source) => source[0];
    
    public static T Last<T>(this T[] source) => source[source.Length - 1];
    public static T First<T>(this T[] source) => source[0];
    
    public static List<Transform> GetAllChildren(this Transform source)
    {
        List<Transform> children = new List<Transform>();
        foreach (Transform child in source)
            children.Add(child);
        return children;
    }
   
    public static void SetLayerRecursibly(this GameObject source, int layer)
    {
        foreach (var item in source.transform.GetComponentsInChildren<Transform>())
            item.gameObject.layer = layer;
    }
    public static void PlayCoroutine(this MonoBehaviour source, ref IEnumerator rutine, Func<IEnumerator> rutineMethod)
    {
        if (rutine != null)
            source.StopCoroutine(rutine);
        rutine = rutineMethod();
        source.StartCoroutine(rutine);
    }

    public static IEnumerator CreateQuickSteppedRutine(this MonoBehaviour source, float duration, DeltaTimeType deltaTime, Action<float> step, Action callback = null)
    {
        Func<float> deltaStep;
        switch (deltaTime)
        {
            default:
            case DeltaTimeType.deltaTime:
                deltaStep = ()=> Time.deltaTime;
                break;
            case DeltaTimeType.fixedDeltaTime:
                deltaStep = ()=> Time.fixedDeltaTime;
                break;
            case DeltaTimeType.unscaledDeltaTime:
                deltaStep = ()=> Time.unscaledDeltaTime;
                break;
            case DeltaTimeType.fixedUnscaledDeltaTime:
                deltaStep = ()=> Time.fixedUnscaledDeltaTime;
                break;
        }

        float t = 0;
        do
        {
            t += deltaStep.Invoke() / duration;
            step.Invoke(t);
            yield return null;
        } while (t<1);
        t = 1;
        step.Invoke(t);
        callback?.Invoke();
    }

    public static IEnumerator NonStopAnimation(this MonoBehaviour source, DeltaTimeType deltaTime, Action<float> stepAnimation)
    {
        Func<float> deltaStep;
        switch (deltaTime)
        {
            default:
            case DeltaTimeType.deltaTime:
                deltaStep = ()=> Time.deltaTime;
                break;
            case DeltaTimeType.fixedDeltaTime:
                deltaStep = ()=> Time.fixedDeltaTime;
                break;
            case DeltaTimeType.unscaledDeltaTime:
                deltaStep = ()=> Time.unscaledDeltaTime;
                break;
            case DeltaTimeType.fixedUnscaledDeltaTime:
                deltaStep = ()=> Time.fixedUnscaledDeltaTime;
                break;
        }

        do
        {
            yield return null;
            stepAnimation.Invoke(deltaStep.Invoke());
        } while (true);
    }

    public static void WaitAndExecute(this MonoBehaviour source, ref IEnumerator rutine, float waitTime, bool isRealTime, Action callback)
    {
        source.PlayCoroutine(ref rutine, () => WaitAndExecuteRutine(source, waitTime, isRealTime, callback));
    }

    public static IEnumerator WaitAndExecuteRutine(this MonoBehaviour source, float waitTime, bool isRealTime, Action callback)
    {
        if (isRealTime)
            yield return new WaitForSecondsRealtime(waitTime);
        else
            yield return new WaitForSeconds(waitTime);

        callback.Invoke();
    }

    public static bool IsBetweenInclusive(this int value, Vector2Int minMax)
    {
        return value >= minMax.x && value <= minMax.y;
    }

    public static bool IsBetweenExclusive(this int value, Vector2Int minMax)
    {
        return value > minMax.x && value < minMax.y;
    }

    public static void Swap(this Vector2 value)
    {
        float aux = value.x;
        value.x = value.y;
        value.y = aux;
    }

    public static void Swap(this Vector2Int value)
    {
        int aux = value.x;
        value.x = value.y;
        value.y = aux;
    }

    /// <summary>
    /// Extension method to check if a layer is in a layermask
    /// </summary>
    /// <param name="mask"></param>
    /// <param name="layer"></param>
    /// <returns></returns>
    public static bool Contains(this LayerMask mask, int layer)
    {
        return mask == (mask | (1 << layer));
    }

    public static Vector2 GetDirection(this Vector2 startPos, Vector2 endPos)
    {
        return GetDifference(startPos, endPos).normalized;
    }

    public static Vector3 GetDirection(this Vector3 startPos, Vector3 endPos)
    {
        return GetDifference(startPos, endPos).normalized;
    }

    public static float GetMagnitud(this Vector2 startPos, Vector2 endPos)
    {
        return GetDifference(startPos, endPos).magnitude;
    }

    public static float GetMagnitud(this Vector3 startPos, Vector3 endPos)
    {
        return GetDifference(startPos, endPos).magnitude;
    }

    public static float GetSqrMagnitud(this Vector2 startPos, Vector2 endPos)
    {
        return GetDifference(startPos, endPos).sqrMagnitude;
    }

    public static float GetSqrMagnitud(this Vector3 startPos, Vector3 endPos)
    {
        return GetDifference((Vector2)startPos, (Vector2)endPos).sqrMagnitude;
    }

    public static float GetLerp(this Vector2 startPos, float t)
    {
        return Mathf.Lerp(startPos.x, startPos.y, t);
    }
    
    public static float GetLerpUnclamped(this Vector2 startPos, float t)
    {
        return Mathf.LerpUnclamped(startPos.x, startPos.y, t);
    }
       
    public static Vector2 GetDifference(this Vector2 startPos, Vector2 endPos)
    {
        return (endPos - startPos);
    }

    public static Vector3 GetDifference(this Vector3 startPos, Vector3 endPos)
    {
        return GetDifference((Vector2)startPos, (Vector2)endPos);
    }

    public static float GetAngle(this Vector2 startPos, Vector2 endPos)
    {
        Vector2 dif = GetDirection(startPos, endPos);
        float angle = Mathf.Atan2(dif.y, dif.x) * Mathf.Rad2Deg;
        return angle;
    }

    public static float AsAngle(this Vector2 source)
    {
        Vector2 normalized = source.normalized;
        float angle = Mathf.Atan2(normalized.y, normalized.x) * Mathf.Rad2Deg;
        return angle;
    }

    public static float AsAngle2D(this Vector3 source)
    {
        return AsAngle((Vector2)source);
    }

    public static Vector2 AsVector(this float degree)
    {
        return RadianToVector2(degree * Mathf.Deg2Rad);
    }

    public static Vector2 RadianToVector2(float radian)
    {
        return new Vector2(Mathf.Cos(radian), Mathf.Sin(radian));
    }
    public static float GetAngle(this Vector3 startPos, Vector3 endPos)
    {
        return GetAngle((Vector2)startPos, (Vector2)endPos);
    }

    public static float GetRandomBetweenXY(this Vector2 value)
    {
        return Mathf.Lerp(value.x, value.y, Random.Range(0, 1f));
    }


    public static Vector2 GetRandomBetween(this Vector2 startPos, Vector3 endpos)
    {
        return Vector2.Lerp(startPos, endpos, Random.Range(0, 1f));
    }

    public static bool IsValueBetween(this Vector2 source, float value) => value >= source.x && value <= source.y;

    public static Vector2 GetRandomBetweenAsRect(this Vector2 startPos, Vector3 endpos)
    {
        return new Vector2(Mathf.Lerp(startPos.x, endpos.x, Random.Range(0f, 1f)), Mathf.Lerp(startPos.y, endpos.y, Random.Range(0f, 1f)));
    }

    public static Vector2 ClampPositionToView(this Camera camera, Vector2 value)
    {
        float hSize = GetHorizontalSize(camera);
        return new Vector2(Mathf.Clamp(value.x, -hSize, hSize), Mathf.Clamp(value.y, -camera.orthographicSize, camera.orthographicSize)) + (Vector2)camera.transform.position;
    }

    public static Vector2 Clamp(this Vector2 value, Vector2 min, Vector2 max)
    {
        return new Vector2(Mathf.Clamp(value.x, min.x, max.x), Mathf.Clamp(value.y, min.y, max.y));
    }

    public static bool IsInsideCameraView(this Camera camera, Vector2 value)
    {
        value -= (Vector2)camera.transform.position;
        return Mathf.Abs(value.y) < camera.orthographicSize && Mathf.Abs(value.x) < camera.GetHorizontalSize();
    }

    public static float GetHorizontalSize(this Camera camera)
    {
        return camera.aspect * camera.orthographicSize * 2;
    }

    public static Vector2 GetOrtographicSize(this Camera camera)
    {
        return new Vector2(camera.GetHorizontalSize(), camera.orthographicSize * 2);
    }

    public static bool ContainsInLocalSpace(this BoxCollider2D boxCollider2D, Vector2 worldSpacePoint)
    {
        worldSpacePoint = boxCollider2D.transform.InverseTransformPoint(worldSpacePoint);
        return Mathf.Abs(worldSpacePoint.x) <= boxCollider2D.size.x / 2 && Mathf.Abs(worldSpacePoint.y) <= boxCollider2D.size.y / 2;
    }


    public static Vector2 GetCropping(this Vector2 fromResolution, float toAspect)
    {
        Vector2 dif = Vector2.zero;
        float aspect = Camera.main.aspect;
        if (toAspect > aspect)
        {
            //Debug.Log("Horizontal");
            float targetHeight = fromResolution.x / toAspect;
            dif.y = (fromResolution.y - targetHeight) / 2;
        }
        else
        {
            //Debug.Log("Vertical");
            float targetWidth = fromResolution.y * toAspect;
            dif.x = (fromResolution.x - targetWidth) / 2;
        }
        return dif;
    }

    public static Rect SetCenter(this ref Rect rect, Vector2 newPos)
    {
        rect.Set(newPos.x - rect.width / 2, newPos.y - rect.height / 2, rect.width, rect.height);
        return rect;
    }


    public static bool LineIntersection(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4, ref Vector2 intersection)
    {

        float Ax, Bx, Cx, Ay, By, Cy, d, e, f, num/*,offset*/;

        float x1lo, x1hi, y1lo, y1hi;

        Ax = p2.x - p1.x;

        Bx = p3.x - p4.x;

        // X bound box test/
        if (Ax < 0)
        {
            x1lo = p2.x;
            x1hi = p1.x;
        }
        else
        {
            x1hi = p2.x;
            x1lo = p1.x;
        }

        if (Bx > 0)
        {
            if (x1hi < p4.x || p3.x < x1lo) return false;
        }
        else
        {
            if (x1hi < p3.x || p4.x < x1lo) return false;
        }

        Ay = p2.y - p1.y;
        By = p3.y - p4.y;

        // Y bound box test//
        if (Ay < 0)
        {
            y1lo = p2.y;
            y1hi = p1.y;
        }
        else
        {
            y1hi = p2.y;
            y1lo = p1.y;
        }

        if (By > 0)
        {
            if (y1hi < p4.y || p3.y < y1lo) return false;
        }
        else
        {
            if (y1hi < p3.y || p4.y < y1lo) return false;
        }

        Cx = p1.x - p3.x;
        Cy = p1.y - p3.y;
        d = By * Cx - Bx * Cy;  // alpha numerator//
        f = Ay * Bx - Ax * By;  // both denominator//
        // alpha tests//
        if (f > 0)
        {
            if (d < 0 || d > f) return false;
        }
        else
        {
            if (d > 0 || d < f) return false;
        }
        e = Ax * Cy - Ay * Cx;  // beta numerator//
        // beta tests //
        if (f > 0)
        {
            if (e < 0 || e > f) return false;
        }
        else
        {
            if (e > 0 || e < f) return false;
        }
        // check if they are parallel
        if (f == 0) return false;
        // compute intersection coordinates //
        num = d * Ax; // numerator //

        //    offset = same_sign(num,f) ? f*0.5f : -f*0.5f;   // round direction //

        //    intersection.x = p1.x + (num+offset) / f;
        intersection.x = p1.x + num / f;

        num = d * Ay;

        //    offset = same_sign(num,f) ? f*0.5f : -f*0.5f;
        //    intersection.y = p1.y + (num+offset) / f;
        intersection.y = p1.y + num / f;
        return true;
    }

    public static Vector2 PerpendicularClockwise(this Vector2 vector2)
    {
        return new Vector2(vector2.y, -vector2.x);
    }

    public static Vector2 PerpendicularCounterClockwise(this Vector2 vector2)
    {
        return new Vector2(-vector2.y, vector2.x);
    }

    public static T GetRandom<T>(this T[] value)
    {
        return value[Random.Range(0, value.Length)];
    }

    public static T GetRandom<T>(this List<T> value)
    {
        return value[Random.Range(0, value.Count)];
    }

    public static List<T> GetRandoms<T>(this List<T> value, int count, bool canRepeat)
    {
        if (!canRepeat && count > value.Count)
            Debug.LogError("Count is Bigger than the list, but we cannot repeat values. Configuration is imposible to fulfil");

        List<T> iterationList = new List<T>(value);
        List<T> retValue = new List<T>();

        while (retValue.Count < count)
        {
            T element = iterationList.GetRandom();
            if (!canRepeat) iterationList.Remove(element);
            retValue.Add(element);
        }
        return retValue;
    }

    public static T GetLast<T>(this List<T> value)
    {
        return value[value.Count - 1];
    }

    public static T GetLast<T>(this T[] value)
    {
        return value[value.Length - 1];
    }

    public static T[] GetElements<T>(this T[] value, int[] indexes) where T : Object
    {
        T[] rValue = new T[indexes.Length];
        for (int i = 0; i < indexes.Length; i++)
        {
            rValue[i] = value[indexes[i]];
        }
        return rValue;
    }

    public static T[] GetElements<T>(this List<T> value, int[] indexes) where T : Object
    {
        return GetElements(value.ToArray(), indexes);
    }

    public static Vector2[] GetVertex(this BoxCollider2D box)
    {
        Vector2[] retValue = new Vector2[4];
        retValue[0] = new Vector2(box.bounds.min.x - box.edgeRadius, box.bounds.min.y - box.edgeRadius);
        retValue[1] = new Vector2(box.bounds.min.x - box.edgeRadius, box.bounds.max.y + box.edgeRadius);
        retValue[2] = new Vector2(box.bounds.max.x + box.edgeRadius, box.bounds.max.y + box.edgeRadius);
        retValue[3] = new Vector2(box.bounds.max.x + box.edgeRadius, box.bounds.min.y - box.edgeRadius);
        return retValue;
    }

    public static List<List<Vector2>> GetAllPaths(this PolygonCollider2D collider)
    {
        List<List<Vector2>> polygons = new List<List<Vector2>>();
        for (int i = 0; i < collider.pathCount; i++)
        {
            polygons.Add(new List<Vector2>(collider.GetPath(i)));
        }
        return polygons;
    }

    public static void SetPaths(this PolygonCollider2D collider2D, List<List<Vector2>> nPaths)
    {
        collider2D.pathCount = nPaths.Count;
        for (int i = 0; i < collider2D.pathCount; i++)
        {
            collider2D.SetPath(i, nPaths[i].ToArray());
        }
    }
    public static float SnapTo(this float value, float snap)
    {
        return Mathf.Round(value / snap) * snap;
    }
    public static Vector3 SnapTo(this Vector3 value, float snap)
    {
        Vector3 retValue = value;
        retValue.x = retValue.x.SnapTo(snap);
        retValue.y = retValue.y.SnapTo(snap);
        retValue.z = retValue.z.SnapTo(snap);
        return retValue;
    }

    public static Vector2 SnapTo(this Vector2 value, float snap)
    {
        Vector2 retValue = value;
        retValue.x = retValue.x.SnapTo(snap);
        retValue.y = retValue.y.SnapTo(snap);
        return retValue;
    }

    public static int DifferenceXtoY(this Vector2Int value)
    {
        return value.y - value.x;
    }

    public static Color ColorFromString(this string value)
    {
        return new Color(
            (value.GetHashCode() / 256f * 3) % 1,
            (value.GetHashCode() / 256f * 6) % 1,
            (value.GetHashCode() / 256f * 3) % 1);
    }

    public static float GetAsMinMaxPercentageFor(this float value, Vector2 minMaxValues, bool capAtOne)
    {
        if (value < minMaxValues.x) return 0;

        if (capAtOne)
        {
            return
                Mathf.Min(
            (value - minMaxValues.x) /
            (minMaxValues.y - minMaxValues.x), 1);
        }
        else
            return
            (value - minMaxValues.x) /
            (minMaxValues.y - minMaxValues.x);
    }


    public static IList<T> CollectComponents<T>(this IList<GameObject> value) where T : Component
    {
        List<T> l = new List<T>();
        foreach (GameObject gameObject in value)
        {
            T aux = gameObject.GetComponent<T>();
            if (aux != null) l.Add(aux);
        }
        return l;
    }

    public static int NextIndex(this int value, int baseModule)
    {
        return (value + baseModule + 1) % baseModule;
    }

    public static int PreviousIndex(this int value, int baseModule)
    {
        return (value + baseModule - 1) % baseModule;
    } 

    public static void DestroyContent<T>(this List<T> value) where T : Object
    {
        if (value.Count == 0) return;
        for (int i = value.Count-1; i >= 0; i--)
            Object.Destroy(value[i]);
    }

}



[System.Serializable]
public class AutoCountdown
{
    public event Action OnCompleted;
    public bool Completed { get; private set; }
    [ShowInInspector] public readonly bool Loop = true;
    [ShowInInspector] public readonly float Duration;
    [ShowInInspector] private float _currentValue = 0;
    public float Progress => _currentValue / Duration; 
    
    public AutoCountdown(float duration, bool loop)
    {
        Duration = duration;
        Completed = duration == 0;
        Loop = loop;
    }
    public void Tick(float delta)
    {
        if(Completed) return;
        _currentValue += delta;
        if (_currentValue >= Duration)
        {
            OnCompleted?.Invoke();
            if (Loop)
                _currentValue = 0;
            else
                Completed = true;
        }
    }

}

public class AutoIndex
{
    public readonly int MaxValue;

    public AutoIndex(int maxValue)
    {
        MaxValue = maxValue;
    }

    public int Current { get; private set; } = 0;
    public int Next()
    {
        Current = (int)Mathf.Repeat(Current + 1, MaxValue);
        return Current;
    }
    public int Previous()
    {
        Current = (int)Mathf.Repeat(Current - 1, MaxValue);
        return Current;
    }
    public void Set(int index)
    {
        if (index < 0)
            throw new Exception("No negative numbers");
        Current = index;
    }
}
