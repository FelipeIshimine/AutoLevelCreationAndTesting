using System;
using System.Collections.Generic;
using UnityEngine;
//using ScreenTransitionSystem;
namespace GameStateMachineCore
{
    public class RootStateExample : GameStateWithAddressableAssets<RootStateExample>
    {
        public static bool PrintDebug = true;

        private static RootStateExample instance;
        public static RootStateExample Instance
        {
            get
            {
                if (instance == null)
                    instance = new RootStateExample();
                return instance;
            }
        }

        public static void Initialize()
        {
            if (PrintDebug) Debug.Log($"<Color=green> RootState->Initialize() </color>");
            Instance.SwitchState(Instance);
            //OnInstantiationProgress += ScreenTransitionSystem.ScreenTransition.SetProgress;
        }

        protected override void _Enter()
        {
            if (PrintDebug) Debug.Log("<Color=green> GameManager State ENTER </color>");
        }

        protected override void _Exit()
        {
            //OnInstantiationProgress -= ScreenTransitionSystem.ScreenTransition.SetProgress;
        }
    }
}