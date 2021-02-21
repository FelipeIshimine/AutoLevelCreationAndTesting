using System;
using Sirenix.OdinInspector;
using UnityEngine;

[System.Serializable]
public class Tile
{
    public Action OnFill;

    public readonly Vector2Int Coordinate;
    public bool IsEmpty => ContentId < 1;
    public bool IsFilled { get; private set; } = false;

    [ShowInInspector] private int _contentId;
    public int ContentId
    {
        get => _contentId;
        set=> _contentId = value;
        
    }

    public Tile(Vector2Int coordinate, int contentId)
    {
        Coordinate = coordinate;
        _contentId = contentId;
    }

    public void Fill()
    {
        if (IsFilled) return;
        if (ContentId != 0)
            return;

        OnFill?.Invoke();
        IsFilled = true;
    }

    public override string ToString() => $"Tile:[{Coordinate.x},{Coordinate.y}]";

   
}
    