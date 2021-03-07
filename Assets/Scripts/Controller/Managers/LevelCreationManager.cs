using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class LevelCreationManager : RuntimeScriptableSingleton<LevelCreationManager>
{
   [ListDrawerSettings(ShowIndexLabels =true)] public List<Level> levels = new List<Level>();
    public static int LevelsCount => Instance.levels.Count;

    public List<Level> discardedLevels = new List<Level>();


    [Button]
    public void OptimizeSizeOfAll()
    {
        foreach (var item in levels)
            item.RemoveExcess();
    }

    [Button]
    public void DiscardLevelsWithoutSolution()
    {
        for (int i = levels.Count-1; i >= 0; i--)
        {
            if(levels[i].Solution.Count == 0)
            {
                discardedLevels.Add(levels[i]);
                levels.RemoveAt(i);
            }
        }
    }

    [Button]
    public void RenameLevels()
    {
        for (int i = 0; i < levels.Count; i++)
            AssetDatabase.RenameAsset(AssetDatabase.GetAssetPath(levels[i]), $"Level {i}");
        for (int i = 0; i < discardedLevels.Count; i++)
            AssetDatabase.RenameAsset(AssetDatabase.GetAssetPath(discardedLevels[i]), $"Discarded_Level {i}");
    }

    [Button] public void Sort() => levels.Sort(LevelSorter);

    [Button] public void SortWithoutMagnitud() => levels.Sort(LevelSorterWithoutMagnitud);
    
    private int LevelSorterWithoutMagnitud(Level x, Level y)
    {
        if (x.Cost > y.Cost) return 1;
        else if (x.Cost < y.Cost) return -1;

        if (x.TimeCost > y.TimeCost) return 1;
        else if (x.TimeCost < y.TimeCost) return -1;

        return 0;
    }

    private int LevelSorter(Level x, Level y)
    {
        if (x.Size.magnitude > y.Size.magnitude) return 1;
        else if (x.Size.magnitude < y.Size.magnitude) return -1;

        if (x.Cost > y.Cost) return 1;
        else if (x.Cost < y.Cost) return -1;

        if (x.TimeCost > y.TimeCost) return 1;
        else if (x.TimeCost < y.TimeCost) return -1;

        return 0;
    }

    internal static void SaveSolution(int levelIndex, List<GridMove> currentSolution)
    {
        Level level = Instance.levels[levelIndex];
        level.Solution = currentSolution.ConvertAll(x=>x.Direction);
    }

    internal static void AddSolution(int levelIndex, List<Vector2Int> moves)
    {
        Level level = Instance.levels[levelIndex];
        level.Solution = moves;
    }

    internal static void AddSolution(int levelIndex, Queue<Vector2Int> moves, int cost, double totalSeconds)
    {
        Level level = Instance.levels[levelIndex];
        level.Solution = new List<Vector2Int>(moves);
        level.Cost = cost;
        level.TimeCost = Convert.ToSingle(totalSeconds);
        AssetDatabase.SaveAssets();
    }

    public static List<Vector2Int> GetSolution(int levelIndex) => Instance.levels[levelIndex].Solution;

    internal static int Add(Vector2Int startPosition, Vector2Int size, List<Tile> list)
    {
        Level nLevel = CreateInstance<Level>();
        nLevel.Initialize(startPosition, size, list);
        Instance.levels.Add(nLevel);
        AssetDatabase.CreateAsset(nLevel, $"Assets/Levels/Level_{Instance.levels.Count.ToString("00000")}.asset");
        return Instance.levels.Count-1;
    }

    internal static Level Get(int index) => Instance.levels[index];
}


