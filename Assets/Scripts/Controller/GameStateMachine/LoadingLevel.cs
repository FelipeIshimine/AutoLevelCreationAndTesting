using GameStateMachineCore;
using System;
using UnityEngine;

internal class LoadingState : GameState<LoadingState>
{
    private int levelIndex;
    private Action callback;
    private bool _useAnimation;

    public LoadingState(int levelIndex, bool useAnimation, Action callback)
    {
        _useAnimation = useAnimation;
        this.callback = callback;
        this.levelIndex = levelIndex;
    }

    public override void Enter()
    {
        base.Enter();
        Debug.Log($"Loading level: {levelIndex}");
        if(!_useAnimation)
        {
            GridView.LoadLevel(levelIndex);
            callback?.Invoke();
        }
        else
        {
            GridView.LoadLevelAnimated(levelIndex, callback);
        }
    }
}