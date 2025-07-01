using System.Collections.Generic;
using UnityEngine;

public class Pool<T> where T : MonoBehaviour, IDeactivatable<T>
{
    private const int MaximumSize = 100;

    private readonly T _prefab;
    private readonly Transform _parent;
    private readonly Stack<T> _elements = new();

    private int _count;
    private int _size;

    public Pool(T prefab, Transform parent, int size = MaximumSize)
    {
        _prefab = prefab;
        _parent = parent;
        _size = size;
    }

    public bool TryGet(out T element)
    {
        element = null;

        if (_elements.Count == 0 && _count >= _size)
            return false;

        if (_elements.Count > 0)
            element = _elements.Pop();
        else
            element = Create();

        element.Deactivated += Return;
        element.gameObject.SetActive(true);

        return true;
    }

    private void Return(T element)
    {
        element.Deactivated -= Return;
        element.gameObject.SetActive(false);
        _elements.Push(element);
    }

    private T Create()
    {
        _count++;

        return Object.Instantiate(_prefab, _parent);
    }
}