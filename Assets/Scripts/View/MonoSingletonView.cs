using UnityEngine;

public abstract class MonoSingletonView<T,V> : MonoSingleton<T> where T : MonoSingletonView<T, V>
{
    [SerializeField] private V _model;
    public V Model { get => _model; set => _model = value; }
}