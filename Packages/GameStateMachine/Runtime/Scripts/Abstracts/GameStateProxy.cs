using UnityEngine;
namespace GameStateMachineCore
{
    public class GameStateProxy : MonoBehaviour
    {
        public BaseGameState GameState { get; private set; }

        public void Initialize<T>(T nGameState) where T : GameState<T>
        {
            GameState = nGameState;
        }
    }
}