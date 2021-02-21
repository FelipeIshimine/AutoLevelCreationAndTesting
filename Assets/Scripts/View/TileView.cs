using System;
using UnityEngine;

public class TileView : MonoBehaviourView<Tile>
{
    public TileContentSettings settings;
    public GameObject content;
    public SpriteRenderer render;

    internal void Initialize(Tile tile)
    {
        Model = tile;
        InstantiateContent();
        name = $"[{tile.Coordinate.x},{tile.Coordinate.y}]";
    }
    public void InstantiateContent()
    {
        if(content!=null)
        {
            if (Application.isPlaying)
                Destroy(content);
            else
                DestroyImmediate(content);
        }
        if (Model.ContentId >= 0)
        {
            content = Instantiate(settings.prefabs[Model.ContentId]);
            content.transform.SetParent(transform);
            content.transform.localPosition = Vector2.zero;
            render = content.GetComponentInChildren<SpriteRenderer>();
            if(Model.ContentId == 0) render.color = Model.IsEmpty ? settings.empty : settings.fill;
        }
    }

    public void Refresh()
    {
        if(Model.IsFilled)
            render.color = settings.fill;
        else if (Model.ContentId == 0) 
            render.color = Model.IsEmpty ? settings.empty : settings.fill;
    }
}
    