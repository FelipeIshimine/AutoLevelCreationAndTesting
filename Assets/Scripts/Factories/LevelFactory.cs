using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public static class LevelFactory    
{
    public static int[,] CreateRandomLevel(Vector2Int size, int range)
    {
        int[,] values = new int[size.x, size.y];
        for (int x = 0; x < size.x; x++)
            for (int y = 0; y < size.y; y++)
                values[x, y] = Random.Range(0, range);
        return values;
    }

    public static int[,] CreateWithBacktracking(Vector2Int size, int maxMoves, Vector2Int startPosition, out List<Vector2Int> moves)
    {
        int[,] values = new int[size.x, size.y];
        for (int x = 0; x < size.x; x++)
            for (int y = 0; y < size.y; y++)
                values[x, y] = -1;

        Grid grid = new Grid(values, Vector2Int.zero);

        moves = new List<Vector2Int>();

        grid[startPosition].ContentId = 0;
        Vector2Int currentPosition = startPosition;
        Vector2Int direction = Vector2Int.zero;

        for (int i = 0; i < maxMoves; i++)
        {
            var validDirections = grid.GetValidDirectionsAt(currentPosition);

            validDirections.AddRange(validDirections);

            if (validDirections.Count == 0)
                break;

            direction = grid.GetValidDirectionsAt(currentPosition).GetRandom();

            List<Vector2Int> totalPath = grid.GetPathFrom(currentPosition, direction, out Vector2Int wall);

            List<int> distanceIndexOptions = new List<int>();  //Arreglo de indices de todas las posiciones
            for (int j = 0; j < totalPath.Count; j++)
                distanceIndexOptions.Add(j+1);  

            int selectedDistance;
            do
            {
                selectedDistance = distanceIndexOptions.GetRandom();
                var next = grid.GetTileRelativeTo(currentPosition, direction * (selectedDistance + 1));
                wall = currentPosition + direction * (selectedDistance + 1);
                if (next != null && next.ContentId == 0)
                {
                    distanceIndexOptions.Remove(selectedDistance);
                    selectedDistance = -1;
                }

            } while (selectedDistance == -1 && distanceIndexOptions.Count > 0);

            if (distanceIndexOptions.Count == 0 || selectedDistance == -1) 
                throw new Exception("No se encontro ninguna opcion validda WTF");

            moves.Add(direction);

            List<Vector2Int> endPath = totalPath.GetRange(0, selectedDistance);
            //string t = string.Empty;

            List<Tile> tiles = grid.GetTiles(endPath);
            foreach (var item in tiles)
            {
                //t += $"{item.Coordinate}>";
                if (item.ContentId == -1)
                    item.ContentId = 0;
            }
           // Debug.Log( $"{currentPosition}: " + t + $" Wall:{wall}: {grid.IsValidCoordinate(wall)}");
            currentPosition = endPath.Last();

            if (grid.IsValidCoordinate(wall))
                grid[wall].ContentId = 1;
        }

        foreach (var item in grid.Tiles)
            if (item.ContentId == -1)
                item.ContentId = 1;

        return grid.GetValues();
    }

       
    public static IEnumerator CreateWithBacktrackingRoutine(Vector2Int size, int maxMoves, Vector2Int startPosition, Action<Vector2Int, int> OnTileModified, Transform guide, float delta = 1)
    {
        int[,] values = new int[size.x, size.y];
        for (int x = 0; x < size.x; x++)
            for (int y = 0; y < size.y; y++)
                values[x, y] = -1;

        Grid grid = new Grid(values, Vector2Int.zero);

        Vector2Int currentPosition = startPosition;
        Vector2Int direction = Vector2Int.zero;
        for (int i = 0; i < maxMoves; i++)
        {
            var validDirections = grid.GetValidDirectionsAt(currentPosition);

            validDirections.AddRange(validDirections);

            if (validDirections.Contains(direction))
                validDirections.Remove(direction);
            
            if (validDirections.Count == 0)
                break;

            direction = grid.GetValidDirectionsAt(currentPosition).GetRandom();
            int maxLenght = grid.GetMaxLenght(currentPosition, direction);

            List<Vector2Int> path = grid.GetPathFrom(currentPosition, direction, out Vector2Int wall);

            List<int> positions = new List<int>();

            for (int j = 1; j < path.Count; j++)
                positions.Add(j);

            Tile currentWall;
            int posIndex=0;

            do 
            {
                int index = Random.Range(0, positions.Count);
                if (positions.Count > 0)
                {
                    posIndex = positions[index];
                    positions.RemoveAt(index);
                    currentWall = grid.GetTileRelativeTo(currentPosition, direction * (posIndex+1));
                }
                else
                    currentWall = null;
            } while (currentWall != null && currentWall.ContentId == 0);

            if (currentWall != null && currentWall.ContentId == 0) //Cancelamos
                continue;



            if(currentWall != null)
                Debug.Log($"Move From:{currentPosition} > {currentWall.Coordinate}={currentWall.ContentId}");
            else
                Debug.Log($"Move From:{currentPosition} > WALL ");

            path = path.GetRange(0, posIndex);

            List<Tile> tiles = grid.GetTiles(path);

            string t = string.Empty;
            foreach (var item in tiles)
                t += t + $"-{item}";
            Debug.Log(t);

            foreach (var item in tiles)
            {
                if (item.ContentId == -1)
                {
                    item.ContentId = 0;
                    OnTileModified?.Invoke(item.Coordinate, 0);
                }
            }


            if (currentWall != null && currentWall.ContentId == -1)
            {
                currentWall.ContentId = 1;
                OnTileModified?.Invoke(currentWall.Coordinate, 1);
            }

            currentPosition += direction * (path.Count);
            guide.transform.position = WorldToGrid(currentPosition, size);

            yield return new WaitForSecondsRealtime(delta);
        }

        foreach (var item in grid.Tiles)
            if (item.ContentId == -1)
            {
                item.ContentId = 1;
                OnTileModified?.Invoke(item.Coordinate, 1);
            }

        yield return grid.GetValues();
    }


    public static List<Vector2Int> GetSolutionWithMovePathFinding(Grid grid)
    {
        List<GridMove> allPosibleMoves = grid.GetAllMoves();

        var emptyTiles = grid.GetAllEmptyTiles();

        bool IsCompletePath(List<GridMove> currentPath)
        {
            return emptyTiles.TrueForAll(x => currentPath.Exists(y => y.Coordinates.Contains(x.Coordinate)));
        }

        int step = 0;
        List<GridMove> Next(List<GridMove> currentPath, List<GridMove> allMoves)
        {
            if (IsCompletePath(currentPath))
                return currentPath;

            var last = currentPath.Last().Last;
            List<GridMove> validMoves = allMoves.FindAll(x => x.SourcePosition == last);

            string str = string.Empty;

            foreach (var coordinate in currentPath)
                str = $"{str}-{coordinate}";

            Debug.Log(str);

            for (int i = 0; i < validMoves.Count; i++)
            {
                currentPath.Add(validMoves[i]);
                allMoves.Remove(validMoves[i]);

                //Debug.Log("CACA");

                var result = Next(currentPath, allMoves);

                if (result != null )
                    return result;

                currentPath.Remove(validMoves[i]);
                allMoves.Add(validMoves[i]);
            }
            return null;
        }

        return Next(new List<GridMove>() { new GridMove(grid.StartPosition, new List<Vector2Int>() { grid.StartPosition }) }, allPosibleMoves).ConvertAll(x=>x.Direction);
    }

   

    //Probar version con historial. Va guardando los pasos que hace hasta que logra hacer X cantidad de pasos, pero si llega  a una cituacion en la que no puede progresar vuelve hasta generar otro camino

    private static void DrawBoxAt(Vector2Int currentPosition, Vector2Int size)
    {
        Vector2 center = WorldToGrid(currentPosition, size);
        Debug.DrawLine(center + new Vector2(-.5f, .5f), center + new Vector2(.5f, .5f));
        Debug.DrawLine(center + new Vector2(.5f, .5f), center + new Vector2(.5f, -.5f));
        Debug.DrawLine(center + new Vector2(.5f, -.5f), center + new Vector2(-.5f, -.5f));
        Debug.DrawLine(center + new Vector2(-.5f, -.5f), center + new Vector2(-.5f, .5f));
    }

    private static Vector2 WorldToGrid(Vector2Int currentPosition, Vector2Int size)
    {
        Vector2 startPosition = -(size-Vector2.one) / 2;
        Vector2 center = startPosition + currentPosition;
        return center;
    }

    public static Queue<Vector2Int> FindBestPath(Grid grid, out int cost)
    {
        PriorityQueue<SolutionPath> pQueue = new PriorityQueue<SolutionPath>();
        int emptyTilesCount = grid.GetAllEmptyTiles().Count;

        pQueue.Enqueue(new SolutionPath(0, new List<Vector2Int>(), new HashSet<Vector2Int>() { grid.StartPosition }, grid.StartPosition, 0));

        Queue<Vector2Int> solution = null;
        cost = 0;
        do
        {
            var current = pQueue.Dequeue();
            if(current.Visited.Count == emptyTilesCount)
            {
                solution = new Queue<Vector2Int>(current.Moves);
                cost = current.Cost;
                break;
            }

            var next = current.GetNext(grid);
            foreach (var item in next)
                pQueue.Enqueue(item);

        } while (pQueue.Count > 0);
        return solution;
    }

    public static IEnumerator FindBestPathRoutine(Grid grid)
    {
        PriorityQueue<SolutionPath> pQueue = new PriorityQueue<SolutionPath>();
        int emptyTilesCount = grid.GetAllEmptyTiles().Count;

        pQueue.Enqueue(new SolutionPath(0, new List<Vector2Int>(), new HashSet<Vector2Int>() { grid.StartPosition }, grid.StartPosition, 0));

        Queue<Vector2Int> solution = null;
        int cost = 0;
        do
        {
            var current = pQueue.Dequeue();
            if (current.Visited.Count == emptyTilesCount)
            {
                solution = new Queue<Vector2Int>(current.Moves);
                cost = current.Cost;
                break;
            }

            var next = current.GetNext(grid);
            foreach (var item in next)
                pQueue.Enqueue(item);

            yield return null;
        } while (pQueue.Count > 0);
        yield return (solution, cost);
    }

}

public class SolutionPath : IComparable<SolutionPath>
{
    public int Cost;
    public List<Vector2Int> Moves;
    public HashSet<Vector2Int> Visited;
    public Vector2Int Last;
    public readonly int Strike;

    public SolutionPath(int cost, List<Vector2Int> moves, HashSet<Vector2Int> visited, Vector2Int last, int strike)
    {
//        Debug.Log($"Created with Last:{last}");
        Cost = cost + strike;
        Strike = strike;

        /*        string mov = string.Empty;
                foreach (var item in moves)
                    mov += $"{item}|";
                Debug.Log(mov);*/

        Moves = moves;
        Visited = visited;
        Last = last;
    }

    public int CompareTo(SolutionPath other)
    {
        if (Cost > other.Cost) return 1;
        else if (Cost < other.Cost) return -1;

        if (Visited.Count < other.Visited.Count) return 1;
        else if (Visited.Count > other.Visited.Count) return -1;
       
        return 0;
    }

    public List<SolutionPath> GetNext(Grid grid)
    {
        var value = new List<SolutionPath>();

        AddNext(grid, value, Vector2Int.left);
        AddNext(grid, value, Vector2Int.right);
        AddNext(grid, value, Vector2Int.up);
        AddNext(grid, value, Vector2Int.down);

        return value;
    }

    private void AddNext(Grid grid, List<SolutionPath> value, Vector2Int dir)
    {
        if (grid.IsValidCoordinate(Last + dir))
        {
//            Debug.Log($"Added:{Last}>{Last + dir}");
            var next = CreateNext(grid, dir);
            if(next != null)
                value.Add(next);
        }
    }

    private SolutionPath CreateNext(Grid grid, Vector2Int direction)
    {
        List<Vector2Int> path = grid.GetPathFrom(Last, direction, out _);

        if (path.Count == 0)
            return null;

        List<Vector2Int> moves = new List<Vector2Int>(Moves);
        moves.Add(direction);

        HashSet<Vector2Int> visited = new HashSet<Vector2Int>(Visited);

        int oldCount = visited.Count;

        foreach (var coordenate in path)
            visited.Add(coordenate);

        bool costUp = (oldCount == visited.Count);

        return new SolutionPath(costUp ? Cost + 1 : Mathf.Max(Cost-1,0), moves, visited, path[path.Count-1], costUp?Strike+1:0);
    }
}

[System.Serializable]
public class GridMove
{
    public readonly List<Vector2Int> Coordinates = new List<Vector2Int>();
    public readonly Vector2Int SourcePosition;
    public readonly Vector2Int Direction;
    public readonly int DirectionAsInt;

    public GridMove(Vector2Int sourcePosition, List<Vector2Int> list)
    {
        SourcePosition = sourcePosition;
        Coordinates = list;
        Direction = list.Count>0?list[0] - sourcePosition:Vector2Int.zero;
        if (Direction == Vector2Int.left) DirectionAsInt = 0;
        else if (Direction == Vector2Int.right) DirectionAsInt = 1;
        else if (Direction == Vector2Int.up) DirectionAsInt = 2;
        else DirectionAsInt = 3;
    }

    public Vector2Int First => Coordinates[0];
    public Vector2Int Last => Coordinates[Coordinates.Count-1];
    public int Count => Coordinates.Count;

    public Vector2Int GetDirection() => Direction;

    public override string ToString()
    {
        string str = $"|{SourcePosition}|";
        foreach (var item in Coordinates)
            str += $"-{item}";
        return str;
    }
}

public class GridBruteForceSolution
{
    public Action<List<GridMove>> OnSolutionFound;
    public readonly Node _root;
    public readonly Queue<Node> _leafs;
    public readonly Grid _grid;
    private readonly HashSet<Vector2Int> EmptyCoordinates;

    public List<GridMove> Solution { get; private set; }

    public GridBruteForceSolution(Grid grid)
    {
        EmptyCoordinates = new HashSet<Vector2Int>(grid.GetAllEmptyTiles().ConvertAll(x=>x.Coordinate));
        _root = new Node(new GridMove(grid.StartPosition, new List<Vector2Int>() { grid.StartPosition }), null, new HashSet<Vector2Int>(), this);
        _grid = grid;
        _leafs = new Queue<Node>();
        _leafs.Enqueue(_root);
    }

    public void FindSolution()
    {
        int i = 0;
        do
            Debug.Log(++i);
        while (!Next());
    }

    public IEnumerator FindSolutionRoutine()
    {
        int i = 0;
        Debug.Log($"StartPos:{_grid.StartPosition}");
        do
        {
            i++;
            Debug.Log($"Step:{++i} Leafs:{_leafs.Count} > Progress:{maxCompletion}");
            yield return null;
        }
        while (!Next());
    }
    float maxCompletion = 0;
    public bool Next()
    {
        /*
          var bestPath = GetBestPath();
          _leafs.Remove(bestPath);
        */ 
        var bestPath = _leafs.Dequeue();
        //Debug.Log(bestPath.GetCompletionRate());
        maxCompletion = Mathf.Max(maxCompletion, bestPath.GetCompletionRate());
        if (bestPath.IsCompleted())
        {
            Done(bestPath);
            return true;
        }

        if (bestPath.Move.Count == 0)
            return false;

        var next = new Node(new GridMove(bestPath.LastPosition,_grid.GetPathFrom(bestPath.LastPosition, Vector2Int.up, out _)), bestPath, bestPath.VisitedCoordinates, this);
        if (next.Move.Count > 0) _leafs.Enqueue(next);

        next = new Node(new GridMove(bestPath.LastPosition, _grid.GetPathFrom(bestPath.LastPosition, Vector2Int.down, out _)), bestPath, bestPath.VisitedCoordinates, this);
        if (next.Move.Count > 0) _leafs.Enqueue(next);

        next = new Node(new GridMove(bestPath.LastPosition, _grid.GetPathFrom(bestPath.LastPosition, Vector2Int.left, out _)), bestPath, bestPath.VisitedCoordinates, this);
        if (next.Move.Count > 0) _leafs.Enqueue(next);

        next = new Node(new GridMove(bestPath.LastPosition, _grid.GetPathFrom(bestPath.LastPosition, Vector2Int.right, out _)), bestPath, bestPath.VisitedCoordinates, this);
        if (next.Move.Count > 0) _leafs.Enqueue(next);
        return false;
    }

    private void Done(Node bestPath)
    {
        Debug.Log($"<color=green> Path Found </color>");
        List<GridMove> moves = new List<GridMove>();

        Node current = bestPath;
        string path = string.Empty;

        while (current != null)
        {
            if(current.Move.GetDirection() != Vector2Int.zero)
                moves.Add(current.Move);
            
            current = current.Previous;
            if(current != null) 
                path = $"{path}-{current.Move.GetDirection()}";
        }
        Debug.Log(path);
        moves.Reverse();

        Solution = moves;
        OnSolutionFound?.Invoke(moves);
    }

    private Node GetBestPath()
    {
        float completeness = -1;
        Node node = null;
        foreach (var item in _leafs)
        {
            float currentCompletion = item.GetCompletionRate();
            Debug.Log($"{item.ToString() } Completion:{currentCompletion}");
            if (currentCompletion >= completeness)
            {
                node = item;
                completeness = currentCompletion;
            }
        }
        return node;
    }

    public class Node
    {
        private readonly GridBruteForceSolution _gridSolution;
        public GridMove Move;
        private HashSet<Vector2Int> EmptyCoordinates => _gridSolution.EmptyCoordinates;

        public Vector2Int LastPosition => Move.Coordinates[Move.Coordinates.Count - 1];

        public readonly HashSet<Vector2Int> VisitedCoordinates;

        public readonly Node Previous;

        public Node(GridMove move, Node previous, HashSet<Vector2Int> visitedCoordinates, GridBruteForceSolution gridSolution)
        {
            Previous = previous;
            Move = move;
            _gridSolution = gridSolution;
            VisitedCoordinates = new HashSet<Vector2Int>(visitedCoordinates);

            foreach (var coordinate in move.Coordinates) 
                VisitedCoordinates.Add(coordinate);

        }

        public bool IsCompleted() => GetCompletionRate() == 1;

        public float GetCompletionRate() => ((float)VisitedCoordinates.Count / EmptyCoordinates.Count);
    }
}


public class PriorityQueue<T> where T : IComparable<T>
{
    private List<T> data;
        
    public PriorityQueue()
    {
        this.data = new List<T>();
    }

    public void Enqueue(T item)
    {
        data.Add(item);
        int childIndex = data.Count - 1; // child index; start at end
        while (childIndex > 0)
        {
            int parentIndex = (childIndex - 1) / 2; // parent index
           
            if (data[childIndex].CompareTo(data[parentIndex]) >= 0) break; // child item is larger than (or equal) parent so we're done
            
            T tmp = data[childIndex]; 
            data[childIndex] = data[parentIndex]; 
            data[parentIndex] = tmp;
            childIndex = parentIndex;
        }
    }

    public T Dequeue()
    {
        // assumes pq is not empty; up to calling code
        int li = data.Count - 1; // last index (before removal)
        T frontItem = data[0];   // fetch the front
        data[0] = data[li];
        data.RemoveAt(li);

        --li; // last index (after removal)
        int pi = 0; // parent index. start at front of pq
        while (true)
        {
            int ci = pi * 2 + 1; // left child index of parent
            if (ci > li) break;  // no children so done
            int rc = ci + 1;     // right child
            if (rc <= li && data[rc].CompareTo(data[ci]) < 0) // if there is a rc (ci + 1), and it is smaller than left child, use the rc instead
                ci = rc;
            if (data[pi].CompareTo(data[ci]) <= 0) break; // parent is smaller than (or equal to) smallest child so done
            T tmp = data[pi]; data[pi] = data[ci]; data[ci] = tmp; // swap parent and child
            pi = ci;
        }
        return frontItem;
    }

    public T Peek()
    {
        T frontItem = data[0];
        return frontItem;
    }


    public int Count => data.Count;

    public override string ToString()
    {
        string s = "";
        for (int i = 0; i < data.Count; ++i)
            s += data[i].ToString() + " ";
        s += "count = " + data.Count;
        return s;
    }

    public bool IsConsistent()
    {
        // is the heap property true for all data?
        if (data.Count == 0) return true;
        int li = data.Count - 1; // last index
        for (int pi = 0; pi < data.Count; ++pi) // each parent index
        {
            int lci = 2 * pi + 1; // left child index
            int rci = 2 * pi + 2; // right child index

            if (lci <= li && data[pi].CompareTo(data[lci]) > 0) return false; // if lc exists and it's greater than parent then bad.
            if (rci <= li && data[pi].CompareTo(data[rci]) > 0) return false; // check the right child too.
        }
        return true; // passed all checks
    } // IsConsistent
} // PriorityQueue