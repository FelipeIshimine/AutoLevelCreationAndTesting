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

    internal void Initialize()
    {
        Model = null;
        InstantiateContent();
        //name = $"[{tile.Coordinate.x},{tile.Coordinate.y}]";
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

        int contentID = (Model != null) ? Model.ContentId : 1;

        if (contentID >= 0)
        {
            content = Instantiate(settings.prefabs[contentID]);
            content.transform.SetParent(transform);
            content.transform.localPosition = Vector2.zero;
            render = content.GetComponentInChildren<SpriteRenderer>();
            if(contentID == 0) render.color =  Model.IsEmpty? settings.empty : settings.fill;
        }
    }

    public void Refresh()
    {
        if (Model == null || Model.IsFilled)
            render.color = settings.fill;
        else if (Model.ContentId == 0) 
            render.color = (Model != null && Model.IsEmpty) ? settings.empty : settings.fill;
    }
}
    