using System.Collections;
using UnityEngine;
using GameStateMachineCore;

public class RootState : GameState<RootState>
{
    private static RootState _instance;


    [RuntimeInitializeOnLoadMethod]
    public static void InitializeMachine()  
    {
        _instance = new RootState();
        _instance.Enter();
    }

    public override void Enter()
    {
        base.Enter();
    }

    public void GoToEditor()
    {
        SwitchState(new MainGameState(true));
    }

    public void GoToGame()
    {
        SwitchState(new MainGameState(false));
    }
}