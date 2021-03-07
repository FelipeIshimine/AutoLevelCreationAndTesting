using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Leguar.TotalJSON;
[System.Serializable]
public class Grid 
{
    public Action<Vector2Int> OnFill;

    public Tile[,] Tiles { get; private set; }
    public Vector2Int Size => new Vector2Int(Tiles.GetLength(0), Tiles.GetLength(1));
    public Vector2Int StartPosition;

    public Tile this[Vector2Int coordinate]
    {
        get
        {
            if (!IsValidCoordinate(coordinate))
                Debug.LogError($"Coordenada invalida:{coordinate}");
            return   Tiles[coordinate.x, coordinate.y];
        }
        set => Tiles[coordinate.x, coordinate.y] = value;
    }

    public Tile this[int x, int y]
    {
        get => Tiles[x,y];
        set => Tiles[x,y] = value;
    }

    #region Constructors
    public Grid(int xSize, int ySize, int defaultValue, Vector2Int startPosition)
    {
        StartPosition = startPosition;
        Tiles = new Tile[xSize, ySize];
        for (int x = 0; x < xSize; x++)
            for (int y = 0; y < ySize; y++)
                Tiles[x, y] = new Tile(new Vector2Int(x,y), defaultValue);
    }
    public Grid(Vector2Int size, int defaultValue, Vector2Int startPosition) : this(size.x, size.y, defaultValue, startPosition) { }

    public Grid(int[,] values, Vector2Int startPosition) : this(values.GetLength(0), values.GetLength(1),0, startPosition)
    {
        for (int x = 0; x < Size.x; x++)
            for (int y = 0; y < Size.y; y++)
                Tiles[x, y].ContentId = values[x, y];
    }

    internal List<Vector2Int> GetValidDirectionsAt(Vector2Int currentPosition)
    {
        List<Vector2Int> directions = new List<Vector2Int>();
        if (IsValidCoordinateAndEmpty(currentPosition + Vector2Int.up)) directions.Add(Vector2Int.up);
        if (IsValidCoordinateAndEmpty(currentPosition + Vector2Int.left)) directions.Add(Vector2Int.left);
        if (IsValidCoordinateAndEmpty(currentPosition + Vector2Int.right)) directions.Add(Vector2Int.right);
        if (IsValidCoordinateAndEmpty(currentPosition + Vector2Int.down)) directions.Add(Vector2Int.down);
        return directions;
    }

    public bool IsValidCoordinateAndEmpty(Vector2Int coordinate) 
        => IsValidCoordinate(coordinate) && this[coordinate].IsEmpty;
    #endregion

    public bool IsValidCoordinate(Vector2Int source) => source.x >= 0 && source.y >= 0 && source.x < Size.x && source.y < Size.y;

    internal int GetMaxLenght(Vector2Int startPosition, Vector2Int direction)
    {
        int distance = 0;
        Vector2Int current = startPosition;
        do
        {
            distance++;
            current += direction;
        }
        while (IsValidCoordinate(current) && this[current].IsEmpty);
        return distance;
    }

    internal Tile GetTileRelativeTo(Vector2Int currentPosition, Vector2Int displacement)
    {
        var endCoordinate = currentPosition + displacement;
        return IsValidCoordinate(endCoordinate)?this[endCoordinate] :null;
    }

    public List<Vector2Int> GetPathFrom(Vector2Int sourceCoordinate, Vector2Int direction, out Vector2Int wall)
    {
        if (!IsValidCoordinate(sourceCoordinate))
            throw new Exception($"Coordenada invalida:{sourceCoordinate} en Grid: {Size}");

        List<Vector2Int> coordinates = new List<Vector2Int>();
        Vector2Int currentCoordinate = sourceCoordinate + direction;

        if (!IsValidCoordinate(currentCoordinate))
        {
            wall = currentCoordinate;
            return coordinates;
        }

        var currentTile = this[currentCoordinate];

        while (currentTile.IsEmpty)
        {
            coordinates.Add(currentCoordinate);
            currentCoordinate += direction;

            if(!IsValidCoordinate(currentCoordinate)) break;

            currentTile = this[currentCoordinate];
        }
        wall = currentCoordinate;
        return coordinates;
    }

    internal void Fill(Vector2Int coordinate)
    {
        //Debug.Log($"Fill:{coordinate}");
        this[coordinate].Fill();
        OnFill?.Invoke(coordinate);
    }

    internal List<GridMove> GetAllMoves()
    {
        Queue<Vector2Int> next = new Queue<Vector2Int>();
        next.Enqueue(StartPosition);

        List<GridMove> moves = new List<GridMove>();

        while (next.Count > 0)
        {
            var current = next.Dequeue();
            List<GridMove> nextMoves = GetMovesFrom(current);
            nextMoves.RemoveAll(x => x.Count == 0);
            foreach (var move in nextMoves)
            {
                if (!moves.Exists(x => x.First == move.First && x.Last == move.Last))
                {
                    moves.Add(move);
                    next.Enqueue(move.Last);
                }
            }
        }
        return moves;
    }

    private List<GridMove> GetMovesFrom(Vector2Int current)
    {
        List<GridMove> nextMoves = new List<GridMove>();

        nextMoves.Add(new GridMove(current, GetPathFrom(current, Vector2Int.left, out _)));
        nextMoves.Add(new GridMove(current, GetPathFrom(current, Vector2Int.right, out _)));
        nextMoves.Add(new GridMove(current, GetPathFrom(current, Vector2Int.up, out _)));
        nextMoves.Add(new GridMove(current, GetPathFrom(current, Vector2Int.down, out _)));
        return nextMoves;
    }

    internal int[,] GetValues()
    {
        int[,] values = new int[Size.x, Size.y];
        for (int x = 0; x < Size.x; x++)
            for (int y = 0; y < Size.y; y++)
                values[x, y] = this[x, y].ContentId;
        return values;
    }

    internal List<Tile> GetTiles(List<Vector2Int> path)
    {
        List<Tile> tiles = new List<Tile>();
        foreach (var item in path)
            tiles.Add(this[item]);
        return tiles;
    }

    public List<Tile> GetAllEmptyTiles()
    {
        List<Tile> emptyTiles = new List<Tile>();
        foreach (var item in Tiles)
            if (item.IsEmpty) emptyTiles.Add(item);
        return emptyTiles;
    }

    public List<Tile> GetAllWalls()
    {
        List<Tile> walls = new List<Tile>();
        foreach (var item in Tiles)
            if (item.ContentId == 1) walls.Add(item);
        return walls;
    }


    public void Load(string json) => Load(JSON.ParseString(json));

    public void Load(Level level)
    {
        StartPosition = level.StartPosition;
        var Size = level.Size;

        Tiles = new Tile[Size.x, Size.y];
        for (int x = 0; x < Size.x; x++)
            for (int y = 0; y < Size.y; y++)
                Tiles[x, y] = new Tile(new Vector2Int(x, y), 0);

        foreach (var coordinate in level.Walls)
            this[coordinate].ContentId = 1;

    }

    public void Load(JSON json)
    {
        StartPosition = JsonConvertion.StringToVector2Int(json.GetString("Start"));
        var Size = JsonConvertion.StringToVector2Int(json.GetString("Size"));
        Tiles = new Tile[Size.x, Size.y];

        for (int x = 0; x < Size.x; x++)
            for (int y = 0; y < Size.y; y++)
                Tiles[x, y] = new Tile(new Vector2Int(x, y), 0);

        JSON tilesData = json.GetJSON("Tiles");
        foreach (var item in tilesData.Keys)
        {
            Vector2Int coordinate = JsonConvertion.StringToVector2Int(item);
            this[coordinate].ContentId = 1;
        }
    }

    internal bool IsComplete()
    {
        foreach (var item in Tiles)
            if(item.ContentId != 1 && !item.IsFilled)
                return false;
        return true;
    }

    public JSON GetSave()
    {
        JSON json = new JSON();
        JSON tiles = new JSON();

        json.Add("Start", JsonConvertion.Vector2IntToJson(StartPosition));
        json.Add("Size", JsonConvertion.Vector2IntToJson(Size));
        for (int x = 0; x < Size.x; x++)
            for (int y = 0; y < Size.y; y++) //Solo se guardan los cuadrados con 1
                if(Tiles[x,y].ContentId == 1)
                    tiles.Add(JsonConvertion.Vector2IntToJson(x, y), Tiles[x, y].ContentId);

        json.Add("Tiles", tiles);
        return json;
    }

    internal List<Vector2Int> GetPathFrom(object lastPosition, Vector2Int up, out Vector2Int _)
    {
        throw new NotImplementedException();
    }
}
    

public static class JsonConvertion 
{
    public static Vector2Int StringToVector2Int(string text)
    {
        var aux = text.Split('|');
        return new Vector2Int(int.Parse(aux[0]), int.Parse(aux[01]));
    }

    public static string Vector2IntToJson(Vector2Int position) => $"{position.x}|{position.y}";

    public static string Vector2IntToJson(int x, int y) => $"{x}|{y}";

}