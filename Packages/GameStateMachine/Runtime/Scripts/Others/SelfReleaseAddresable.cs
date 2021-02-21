using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;
namespace GameStateMachineCore
{
    public class SelfReleaseAddresable : MonoBehaviour
    {
        public IReleaseAddressable owner;

        public void ReleaseFromGameStateOwner()
        {
            owner.Release(this);
        }

        internal void Initialize(IReleaseAddressable gameStateWithAddressableAssets)
        {
            owner = gameStateWithAddressableAssets;
        }
    }
}