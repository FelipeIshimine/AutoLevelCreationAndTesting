using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainGameInitializer : MonoBehaviour
{
    private void Start()
    {
        RootState.Instance.GoToGame();
    }
}