using GameStateMachineCore;
using UnityEngine;
using Random = UnityEngine.Random;

public class MainGameState : GameState<MainGameState>
{
    private int currentLevelIndex;

    public readonly bool IsEditor;

    public MainGameState(bool isEditor)
    {
        IsEditor = isEditor;
    }

    public override void Enter()
    {
        base.Enter();

        if (IsEditor)
            GoToGameplay(0);
        else
            GoToGameplay(PlayerPrefs.GetInt("LastLevel", 0));
    }

    private void GoToRandomLevel()
    {
        GoToGameplay(Random.Range(0, LevelCreationManager.Instance.levels.Count));
    }

    public void GoToGameplay(int levelIndex)
    {
        currentLevelIndex = levelIndex;
        SwitchState(new LoadingState(levelIndex, !IsEditor, () => SwitchState(new GameplayState(this, levelIndex))));
    }

    internal void GoNextLevel()
    {
        GoToGameplay((currentLevelIndex + 1) % LevelCreationManager.Instance.levels.Count);
    }

    internal void Restart()
    {
        GoToGameplay(currentLevelIndex);
    }
}


