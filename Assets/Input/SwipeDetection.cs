using System;
using UnityEngine;

public class SwipeDetection : MonoBehaviour
{
    public static Action<Vector2> OnSwipe;
    public static Action<Vector2Int> OnDiscreteSwipe;

    [SerializeField] private float minDistance = .2f;
    [SerializeField] private float maxTime = .2f;
    [SerializeField,Range(0,1)] private float directionThreshold = .9f;

    private InputController inputController;

    private Vector2 startPosition;
    private float startTime;

    private Vector2 endPosition;
    private float endTime;

    private void Awake()
    {
        inputController = InputController.Instance;
    }

    private void OnEnable()
    {
        inputController.OnStartTouch += SwipeStart;
        inputController.OnEndTouch += SwipeEnd;
    }
  
    private void OnDisable()
    {
        inputController.OnStartTouch -= SwipeStart;
        inputController.OnEndTouch -= SwipeEnd;
    }

    private void SwipeStart(Vector2 position, float time)
    {
        startPosition = position;
        startTime = time;
    }

    private void SwipeEnd(Vector2 position, float time)
    {
        endPosition = position;
        endTime = time;
        DetectSwipe();
    }

    private void DetectSwipe()
    {
        if (Vector3.Distance(startPosition,endPosition) >= minDistance && (endTime-startTime) <= maxTime)
        {
            Vector2 direction = endPosition - startPosition;
            direction.Normalize();
            OnSwipe?.Invoke(direction);
            SwipeDirection(direction);
        }

    }

    private void SwipeDirection(Vector2 direction)
    {
        if (Vector2.Dot(Vector2.up, direction) > directionThreshold)
        {
            OnDiscreteSwipe?.Invoke(Vector2Int.up);
            return;
        }
        if (Vector2.Dot(Vector2.left, direction) > directionThreshold)
        {
            OnDiscreteSwipe?.Invoke(Vector2Int.left);
            return;
        }
        if (Vector2.Dot(Vector2.right, direction) > directionThreshold)
        {
            OnDiscreteSwipe?.Invoke(Vector2Int.right);
            return;
        }
        if (Vector2.Dot(Vector2.down, direction) > directionThreshold)
        {
            OnDiscreteSwipe?.Invoke(Vector2Int.down);
            return;
        }
    }
}
