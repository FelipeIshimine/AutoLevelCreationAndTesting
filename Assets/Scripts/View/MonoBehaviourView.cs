using UnityEngine;

public class MonoBehaviourView<T> : MonoBehaviour
{
    [SerializeField] private T _model;
    public T Model { get=>_model; set => _model = value; }
}
