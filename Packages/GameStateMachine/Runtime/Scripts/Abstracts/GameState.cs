using System;
using System.Collections.Generic;
using UnityEngine;
namespace GameStateMachineCore
{
    public abstract class GameState<T> : BaseGameState where T : GameState<T>
    {
        public static T Instance;

        public delegate void GameStateEvent(T state);
        public static event GameStateEvent OnEnter;
        public static event GameStateEvent OnExit;
        
        private BaseGameState _currentState;
        public IState CurrentState => _currentState;

        private static readonly List<IUse<T>> Listeners = new List<IUse<T>>();

        public override void Exit()
        {
             if (_currentState == this)
                throw new Exception("Recursive State");
             
             StatesStack.Pop();

             _currentState?.Exit();
             OnExit?.Invoke(this as T);
             Listeners.ForEach(x=> x.GameState = null);
             
             //Debug.Log($"|EXIT| <Color=brown>  {this} </color>");
        }

        public override void Enter()
        {
            if (_currentState == this)
                throw new Exception("Recursive State");
            Instance = this as T;
            
            StatesStack.Push(this);
            //Debug.Log($"|ENTER| <color=green> {this} </color>");
                
            Listeners.ForEach(x=> x.GameState = this as T);
            OnEnter?.Invoke(this as T);
        }

        public override void SwitchState(BaseGameState nState)
        {
            //Debug.Log($"> <color=teal> {this.GetType().FullName}: </color>");
            Debug.Log($"> <color=teal> { this.GetType().FullName }: </color> <Color=brown> {_currentState?.GetType().Name} </color> => <Color=green> {nState.GetType().Name} </color>");
            if (_currentState != this)
                _currentState?.Exit();
            
            _currentState = nState;
            _currentState?.Enter();
        }

        public override void ExitSubState()
        {
            _currentState.Exit();
            _currentState = null;
        }

        public static void Register(IUse<T> useState)
        {
            Listeners.Add(useState);
            OnEnter += useState.OnEnter;
            OnExit += useState.OnExit;
        }
        
        public static void Unregister(IUse<T> useState)
        {
            Listeners.Remove(useState);
            OnEnter -= useState.OnEnter;
            OnExit -= useState.OnExit;
            useState.GameState = null;
        }
    }
    
    public abstract class BaseGameState : IState
    {
        public static IReadOnlyList<BaseGameState> ActiveStates => StatesStack.ToArray();
        protected static readonly Stack<BaseGameState> StatesStack = new Stack<BaseGameState>();
        public abstract void Enter();
        public abstract void Exit();
        public abstract void SwitchState(BaseGameState nState);

        public static  BaseGameState UpperState=> StatesStack.Peek();

        public abstract void ExitSubState();
    }

    public interface IUse<T> where T : GameState<T>
    {
        T GameState { get; set; }
        void OnEnter(T state);
        void OnExit(T state);
    }
    
}
