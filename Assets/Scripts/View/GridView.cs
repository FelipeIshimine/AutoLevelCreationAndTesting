using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;
using Unity.EditorCoroutines.Editor;
using System;
using Random = UnityEngine.Random;
using System.Collections;
using System.Diagnostics;
using Debug = UnityEngine.Debug;
#if UNITY_EDITOR
using UnityEditor;
#endif 

public class GridView : MonoSingletonView<GridView, Grid>
{
    public GameObject tilePrefab;


    public TileView[,] tiles;


    
    //public List<TileView> activeTiles = new List<TileView>();
    public List<TileView> tileViews = new List<TileView>();

    public Transform player;
    public static Transform Player => Instance.player;

    public static Vector2Int StartPosition => Instance.Model.StartPosition;

    private Vector2 _gridStartPosition;

    [Range(0,1)]public float separationPercentage = 1;
    public float loadLevelAnimationDuration = 1;

    public AnimationCurve sizeCurve = AnimationCurve.EaseInOut(0,0,1,1);

    public float rotationMultiplicator = 1;
    public AnimationCurve rotationCurve = AnimationCurve.EaseInOut(0,0,1,1);

    public Vector2 positionMultiplicator = new Vector2(1, 1);
    public AnimationCurve positionCurveY = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public AnimationCurve positionCurveX = AnimationCurve.EaseInOut(0, 0, 1, 1);


    IEnumerator _routine;

    internal static void LoadLevelAnimated(int levelIndex, Action callback)
    {
        LoadLevel(levelIndex);

        Instance.PlayCoroutine(ref Instance._routine, ()=> Instance.LoadLevelAnimation(callback));
    }

    IEnumerator LoadLevelAnimation(Action callback)
    {
        float t = 0;

        float separation = separationPercentage * tiles.Length;
        float fullSteps = (float)1 / ((float)tiles.Length + 1*separation);

        float individualStep = fullSteps * separation;

        float aux = 1 / loadLevelAnimationDuration;

        Vector2[,] startPositions = new Vector2[tiles.GetLength(0), tiles.GetLength(1)];

        for (int x = 0; x < startPositions.GetLength(0); x++)
            for (int y = 0; y < startPositions.GetLength(1); y++)
                startPositions[x, y] = (Vector2)tiles[x, y].transform.position - positionMultiplicator;

        int xLength = startPositions.GetLength(0);
        int yLength = startPositions.GetLength(1);


        do
        {
            t += Time.deltaTime * aux;
            int i = -1;

            for (int x = 0; x < startPositions.GetLength(0); x++)
                for (int y = 0; y < startPositions.GetLength(1); y++)
                {
                    var item = tiles[x, y];
                    i++;
                    float nt = (t - i * fullSteps) / individualStep;
                    item.transform.localScale = sizeCurve.Evaluate(nt) * Vector3.one;
                    item.transform.rotation = Quaternion.Euler(0, 0, rotationCurve.Evaluate(nt) * rotationMultiplicator);

                    item.transform.position = startPositions[x, y] + new Vector2(positionMultiplicator.x * positionCurveX.Evaluate(nt), positionMultiplicator.y * positionCurveY.Evaluate(nt));
                    //Debug.Log($"{i}: {nt}");
                } 

            yield return null;
        } while (t<1);

        callback?.Invoke();
    }

    internal static Vector2 GetPositionFromCoordinate(Vector2Int from) => Instance._gridStartPosition + from;

    #region Initializers
    public void Initialize(Vector2Int size, int defaultValue, Vector2Int startPosition)
    {
        Model = new Grid(size, defaultValue, startPosition);
        InitializeTiles();
    }
    public void Initialize(int[,] values, Vector2Int startPosition)
    {
        Model = new Grid(values, startPosition);
        InitializeTiles();
    }

    public void InitializeTiles()
    {
        Clear();
        tiles = new TileView[Model.Size.x, Model.Size.y];

        _gridStartPosition = -((Vector2)Model.Size - Vector2Int.one) / 2;

        for (int x = 0; x < Model.Size.x; x++)
        {
            for (int y = 0; y < Model.Size.y; y++)
            {
                GameObject go;
                if (Application.isPlaying)
                    go = Instantiate(tilePrefab);
                else
                    go = PrefabUtility.InstantiatePrefab(tilePrefab) as GameObject;

                var tileView = go.GetComponent<TileView>();
                tiles[x, y] = tileView;
                this.tileViews.Add(tileView);


                tileView.Initialize(Model[x, y]);
                tileView.transform.position = _gridStartPosition + new Vector2(x, y);
                tileView.transform.SetParent(transform);
            }
        }
    }

    #endregion

    bool wait = false;
    bool Wait => wait;

    public IEnumerator ResolveRoutine(Queue<Vector2Int> values, Action<Queue<Vector2Int>> onDone)
    {
        string activeSolution = string.Empty;

        foreach (var item in values)
            activeSolution += $"{item}";

        void Continue() => wait = false;
        int i = 0;
        int max = values.Count;

        while (values.Count > 0)
        {
            i++;
            wait = true;

            var current = values.Dequeue();
            if (current == Vector2.zero)
                continue;

            GameplayState.Playing.Instance.Move(current, Continue);

            yield return new WaitWhile(() => Wait);
            if (Model.IsComplete())
                break;
        }
        onDone?.Invoke(values);
    }

    internal static void RefreshTileViews(List<Vector2Int> path)
    {
        foreach (var item in path)
            RefreshTileView(item);
    }

    internal static void RefreshTileView(Vector2Int playerCoordinate)
    {
        Instance.tiles[playerCoordinate.x, playerCoordinate.y].Refresh();
    }

    [Button]
    public void PlayEveryLevel() => StartCoroutine(PlayEveryLevelRoutine());

    IEnumerator PlayEveryLevelRoutine()
    {
        for (int i = 0; i < LevelCreationManager.LevelsCount; i++)
        {
            Level level = LevelCreationManager.Get(i);
            GoToLevel(i);
            yield return ResolveRoutine(new Queue<Vector2Int>(level.Solution), null);
        }
    }

    [Button]
    public void CreateQuick(List<LevelCreationSettings> settings, bool searchSolution, bool showProcess)
    {
        StartCoroutine(CreateLevelsBatchRoutine(settings, searchSolution, showProcess));
    }

    [Button]
    public void CreateFullyRandom(Vector2Int minSize, Vector2Int maxSize, int maxMoves = 100, int count = 100)
    {
        Grid grid = null;
        for (int i = 0; i < count; i++)
        {
            CreateRandomLevel(new Vector2Int(Random.Range(minSize.x, maxSize.x), Random.Range(minSize.y, maxSize.y)), maxMoves, ref grid);
        }
    }

    [Button]
    public void CreateFullyRandomAndSolve(Vector2Int minSize, Vector2Int maxSize, int maxMoves = 100, int count = 100)
    {
        StartCoroutine(CreateFullyRandomAndSolveRoutine(minSize, maxSize, maxMoves, count));
    }

    IEnumerator CreateFullyRandomAndSolveRoutine(Vector2Int minSize, Vector2Int maxSize, int maxMoves = 100, int count = 100)
    {
        Grid grid = null;
        for (int i = 0; i < count; i++)
        {
            int index = CreateRandomLevel(new Vector2Int(Random.Range(minSize.x, maxSize.x), Random.Range(minSize.y, maxSize.y)), maxMoves, ref grid);
            GoToLevel(index);
            yield return ResolveRoutine(FindSolutionAndSave(), null);
        }
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    [Button]
    public void SolveEveryLevelWithoutSolution(bool show, int maxDuration = 2)=> StartCoroutine(ResolveAllLevelsRoutine(show, maxDuration));

    private IEnumerator ResolveAllLevelsRoutine(bool show, int maxDuration = 2)
    {
        IEnumerator findSolutionRoutine = null;
        List<Level> canceledLevels = new List<Level>();

        Grid grid = null;

        for (int i = 0; i < LevelCreationManager.LevelsCount; i++)
        {
            Level level = LevelCreationManager.Get(i);

            if (level.Solution.Count == 0)
            {
                grid = new Grid(level.Size.x,level.Size.y,0,level.StartPosition);
                grid.Load(level);

                findSolutionRoutine = LevelFactory.FindBestPathRoutine(grid);
                Stopwatch stopwatch = new Stopwatch();

                stopwatch.Start();
                bool cancel = false;
                while (findSolutionRoutine.MoveNext())
                {
                    if (stopwatch.Elapsed.Seconds > maxDuration)
                    {
                        cancel = true;
                        canceledLevels.Add(level);
                        break;
                    }
                }

                if (cancel)
                {
                    Debug.Log($"{i}: <color=red> Canceled </color>");
                    continue;
                }

                (Queue<Vector2Int> solution, int cost) = ((Queue<Vector2Int> solution, int cost))findSolutionRoutine.Current;
                LevelCreationManager.AddSolution(i, solution, cost, stopwatch.Elapsed.TotalMilliseconds / 1000);

                Debug.Log($"{i}: <color=green> Solved </color>in {stopwatch.Elapsed.TotalMilliseconds / 1000}");

                if (show)
                {
                    GameplayState.Instance.GoToLevel(i);
                    yield return ResolveRoutine(solution, null);
                }
                else yield return null;
            }
            Debug.Log($"{i}/{LevelCreationManager.LevelsCount}");
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    
    IEnumerator CreateLevelsBatchRoutine(List<LevelCreationSettings> settings, bool searchSolution, bool showProcess)
    {
        int i = -1;
        Grid grid = null;
        foreach (var item in settings)
        {
            Debug.Log($"Progress:{(float)(++i) / settings.Count}");
            for (int j = 0; j < item.count; j++)
            {
                yield return null;

                int index = CreateRandomLevel(item.size, item.maxMoves,ref grid);
                if(searchSolution)
                {
                    Stopwatch st = new Stopwatch();
                    st.Start();
                    var values = LevelFactory.FindBestPath(grid, out int cost);
                    st.Stop();

                    if (showProcess)
                    {
                        GameplayState.Instance.GoToLevel(index);
                        yield return ResolveRoutine(values, null);
                    }

                    Debug.Log($"Solution found in {st.Elapsed.TotalSeconds}");
                    LevelCreationManager.AddSolution(index, values, cost, st.Elapsed.TotalSeconds);
                }
            }
        }
        Debug.Log("COMPLETED");
    }


    [Button]
    public void CreateReverseRandomLevelAndSave(int x = 6, int y = 6, int maxMoves = 100, int count = 1)
    {
        Grid grid=null;
        for (int i = 0; i < count; i++)
            CreateRandomLevel(new Vector2Int(x,y), maxMoves, ref grid);
    }

    private int CreateRandomLevel(Vector2Int size, int maxMoves, ref Grid grid)
    {
        Vector2Int startPosition = new Vector2Int(Random.Range(0, size.x), Random.Range(0, size.y));
        List<Vector2Int> moves;

        grid = new Grid(LevelFactory.CreateWithBacktracking(
                size,
                maxMoves,
                startPosition,
                out moves),
            startPosition);

        int index = LevelCreationManager.Add(startPosition, size, grid.GetAllWalls());
        return index;
    }

    public void LoadFromManager(int index)
    {
        Model = new Grid(0, 0, 0, Vector2Int.zero);
        Model.Load(LevelCreationManager.Get(index));
        InitializeTiles();
    }

    public void UpdateTile(Vector2Int coordinate, int value)
    {
        Model[coordinate].ContentId = value;
        tiles[coordinate.x, coordinate.y].InstantiateContent();
    }

    public void Clear()
    {
        for (int i = 0; i < tileViews.Count; i++)
        {
            if (Application.isPlaying)
                Destroy(tileViews[i].gameObject);
            else
                DestroyImmediate(tileViews[i].gameObject);
        }
        tileViews.Clear();
    }

    internal static void LoadLevel(int levelIndex) => Instance.LoadFromManager(levelIndex);

    public List<Vector2Int> GetPathFrom(Vector2Int from, Vector2Int direction)
    {
        return Model.GetPathFrom(from, direction, out _);
    }

    [Button]
    public void GetSolutionWithPriorityQueue(bool advanceToNext)
    {
        Queue<Vector2Int> values = FindSolutionAndSave();

        StartCoroutine(ResolveRoutine(new Queue<Vector2Int>(values), null));
    }

    private Queue<Vector2Int> FindSolutionAndSave()
    {
        Stopwatch st = new Stopwatch();
        st.Start();
        var values = LevelFactory.FindBestPath(Model, out int cost);
        st.Stop();
        Debug.Log($"Solution found in {st.Elapsed.TotalSeconds}");
        LevelCreationManager.AddSolution(GameplayState.Instance.LevelIndex, values, cost, st.Elapsed.TotalSeconds);
        return values;
    }

   

    [Button]
    public void GoToLevel(int level)
    {
        MainGameState.Instance.GoToGameplay(level);
    }
}

[System.Serializable]
public class LevelCreationSettings
{
    public Vector2Int size = new Vector2Int(6,6);
    public int maxMoves = 100;
    public int count = 10;
}