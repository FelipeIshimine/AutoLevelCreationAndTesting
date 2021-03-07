using Leguar.TotalJSON;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Level : ScriptableObject, ISaveAsJson
{
    public Vector2Int StartPosition;
    public List<Vector2Int> Walls;
    public Vector2Int Size;

    public List<Vector2Int> Solution;
    public int Cost;
    public float TimeCost;

    public void Initialize(Vector2Int startPosition, Vector2Int size, List<Tile> list)
    {
        StartPosition = startPosition;
        Size = size;
        Walls = list.ConvertAll(x=>x.Coordinate);
        RemoveExcess();
    }

    public void RemoveExcess()
    {
        Vector2Int lowerPosition = FindLowerEmptySpace();
        StartPosition -= lowerPosition;
        for (int i = 0; i < Walls.Count; i++)
            Walls[i] = Walls[i] - lowerPosition;

        Vector2Int upperPosition = FindUpperEmptySpace();

        Size = upperPosition + Vector2Int.one;

        Walls.RemoveAll(item => item.x >= Size.x || item.y >= Size.y || item.x < 0 || item.y < 0);
    }

    private Vector2Int FindUpperEmptySpace()
    {
        Vector2Int upperPosition = new Vector2Int(0,0);
        for (int x = 0; x < Size.x; x++)
            for (int y = 0; y < Size.y; y++)
                if (!Walls.Contains(new Vector2Int(x, y)))
                    upperPosition = new Vector2Int(Mathf.Max(upperPosition.x, x), Mathf.Max(upperPosition.y, y));
        return upperPosition;
    }

    private Vector2Int FindLowerEmptySpace()
    {
        Vector2Int lowerPosition = new Vector2Int(int.MaxValue, int.MaxValue);
        for (int x = 0; x < Size.x; x++)
            for (int y = 0; y < Size.y; y++)
                if(!Walls.Contains(new Vector2Int(x, y)))
                    lowerPosition = new Vector2Int(Mathf.Min(lowerPosition.x, x), Mathf.Min(lowerPosition.y, y));
        return lowerPosition;
    }

    public JSON GetSave()
    {
        JSON json = new JSON();
        JSON tiles = new JSON();

        json.Add("Start", JsonConvertion.Vector2IntToJson(StartPosition));
        json.Add("Size", JsonConvertion.Vector2IntToJson(Size));

        foreach (var wall in Walls)
            tiles.Add(JsonConvertion.Vector2IntToJson(wall.x, wall.y), 1);

        json.Add("Tiles", tiles);


        JArray solution = new JArray();

        foreach (var step in Solution)
            solution.Add(JsonConvertion.Vector2IntToJson(step.x, step.y));

        json.Add("Solution", solution);

        json.Add("Cost", Cost);
        json.Add("TimeCost", TimeCost);

        return json;
    }

    public void Load(JSON data)
    {
        StartPosition = JsonConvertion.StringToVector2Int(data.GetString("Start"));
        Size = JsonConvertion.StringToVector2Int(data.GetString("Size"));
        Walls = new List<Vector2Int>();
       
        JSON tiles = data.GetJSON("Tiles");

        for (int x = 0; x < Size.x; x++)
            for (int y = 0; y < Size.y; y++)
            {
                string key = JsonConvertion.Vector2IntToJson(x, y);
                if(tiles.ContainsKey(key) && tiles.GetInt(key) == 1)
                        Walls.Add(JsonConvertion.StringToVector2Int(key));
            }

        JArray solution = data.GetJArray("Solution");
        Solution.Clear();
        foreach (JString step in solution.Values)
            Solution.Add(JsonConvertion.StringToVector2Int(step.AsString()));

    }
}


