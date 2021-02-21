using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EditorInitializer : MonoBehaviour
{
    private void Start()
    {
        RootState.Instance.GoToEditor();
    }
}
