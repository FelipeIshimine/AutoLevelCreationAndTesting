using System.Collections.Generic;
using UnityEngine;
using GameStateMachineCore;
using System;
using Sirenix.OdinInspector;

public class GameplayState : GameState<GameplayState>
{
    public static event Action OnWin;
    public readonly int LevelIndex;
    public Vector2Int playerCoordinate;
    private List<Vector2Int> emptyTiles;

    private MainGameState mainMenuState;

    public GameplayState(MainGameState mainMenuState, int levelIndex)
    {
        this.mainMenuState = mainMenuState;
        LevelIndex = levelIndex;
    }

    public override void Enter()
    {
        base.Enter();

        End.Win.OnNextLevelRequest += OnNextLevelRequest;

        Canvas_Gameplay.OnRestart += OnRestart;
        Canvas_Gameplay.OnLevelRequest += OnLevelRequest;
        Canvas_Gameplay.OnSkip += OnSkip;

        SetPlayerCoordinate(GridView.StartPosition, !mainMenuState.IsEditor, () => SwitchState(new Playing(this)));

        emptyTiles = GridView.Instance.Model.GetAllEmptyTiles().ConvertAll(x=>x.Coordinate);
        GridView.Instance.Model.OnFill += OnFill;

        GridView.Instance.Model.Fill(playerCoordinate);
        GridView.RefreshTileView(playerCoordinate);


    }

    public override void Exit()
    {
        base.Exit();
        End.Win.OnNextLevelRequest -= OnNextLevelRequest;

        GridView.Instance.Model.OnFill -= OnFill;
        Canvas_Gameplay.OnLevelRequest -= OnLevelRequest;

        Canvas_Gameplay.OnRestart -= OnRestart;
        Canvas_Gameplay.OnSkip -= OnSkip;
    }

    internal void GoToLevel(int index) => OnLevelRequest(index);


    [Button]
    private void OnLevelRequest(int levelIndex)
    {
        mainMenuState.GoToGameplay(levelIndex);
    }

    [Button]
    private void OnSkip()
    {
        mainMenuState.GoNextLevel();
    }

    [Button]
    private void OnRestart()
    {
        mainMenuState.Restart();
    }

    private void OnNextLevelRequest()
    {
        mainMenuState.GoNextLevel();
    }

    private void OnFill(Vector2Int obj)
    {
        emptyTiles.Remove(obj);
    }

    public bool IsLevelComplete() => emptyTiles.Count == 0;
    
    public void Win()
    {
        OnWin?.Invoke();
        SwitchState(new End(this, true, LevelIndex));
    }

    private void SetPlayerCoordinate(Vector2Int coordinate, bool value, Action callback = null)
    {
        playerCoordinate = coordinate;
        PlayerView.SetAtCoordinate(coordinate, value, callback);
    }

    public class Playing : GameState<Playing>
    {
        public static Action<Vector2Int> OnMoveRequest;

        private GameplayState gameplayState;
        public Vector2Int PlayerCoordinate => gameplayState.playerCoordinate;
          

        public Playing(GameplayState gameplayState)
        {
            this.gameplayState = gameplayState;
        }
              
        public override void Enter()
        {
            base.Enter();

            InputController.Enable();
            SwipeDetection.OnDiscreteSwipe += Move;
            OnMoveRequest += Move;
        }

        public override void Exit()
        {
            base.Exit();
            InputController.Disable();
            SwipeDetection.OnDiscreteSwipe -= Move;
            OnMoveRequest -= Move;

        }

        public void Move(Vector2Int direction) => Move(direction, WinCheck);

        private void WinCheck()
        {
            if (gameplayState.IsLevelComplete())
                gameplayState.Win();
        }

        public void Move(Vector2Int direction, Action callback)
        {
            var path = GridView.Instance.Model.GetPathFrom(PlayerCoordinate, direction, out Vector2Int wall);

            if (path.Count < 1) return;

            var endCoordinate = path[path.Count-1];
            var startCoordinate = PlayerCoordinate;

            gameplayState.SetPlayerCoordinate(endCoordinate,false, null);

            InputController.Disable();

            foreach (var item in path)
                GridView.Instance.Model.Fill(item);

            void OnProgress(float t)
            {
                int count = Mathf.FloorToInt((path.Count) * t);
                for (int i = 0; i < count; i++)
                    GridView.RefreshTileView(path[i]);
            }

            PlayerView.Dash(startCoordinate, endCoordinate, OnProgress,
                ()=>
                {
                    InputController.Enable();
                    callback?.Invoke();
                });
        }
    }

    public class End : GameState<End>
    {
        private GameplayState gameplayState;
        private bool isWin;
        public readonly int LevelIndex; 
        public End(GameplayState gameplayState, bool v, int levelIndex)
        {
            this.gameplayState = gameplayState;
            this.isWin = v;
            LevelIndex = levelIndex;
        }

        public override void Enter()
        {
            base.Enter();

            PlayerView.Close();

            if(isWin)
                SwitchState(new Win(LevelIndex));
            else
                SwitchState(new Lose());
        }

        public class Win : GameState<Win>
        {
            public static Action OnNextLevelRequest;

            public readonly int LevelIndex;

            public Win(int levelIndex)
            {
                LevelIndex = levelIndex;
            }

            public override void Enter()
            {
                base.Enter();
                PlayerPrefs.SetInt("LastLevel", LevelIndex);
                OnNextLevelRequest?.Invoke();
            }
        }

        public class Lose : GameState<Lose>
        {

        }
    }
   
}


