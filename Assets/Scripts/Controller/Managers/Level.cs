using Leguar.TotalJSON;
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


