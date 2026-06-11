using UnityEngine;
using System.Collections.Generic;
using System;

public abstract class EventChannelBase : ScriptableObject
{
    public abstract void ClearListeners();
}

public abstract class EventChannelBase<T> : EventChannelBase
{
    private readonly SortedList<int, List<Action<T>>> _priorityListeners = new SortedList<int, List<Action<T>>>();

    public void AddListener(Action<T> action, int priority = 0)
    {
        if (!_priorityListeners.ContainsKey(priority))
        {
            _priorityListeners.Add(priority, new List<Action<T>>());
        }
        _priorityListeners[priority].Add(action);
    }

    public void RemoveListener(Action<T> action)
    {
        foreach (var list in _priorityListeners.Values)
        {
            if (list.Contains(action))
            {
                list.Remove(action);
            }
        }
    }

    public void RaiseEvent(T value)
    {
        foreach (var list in _priorityListeners.Values)
        {
            foreach (var listener in list)
            {
                listener?.Invoke(value);
            }
        }
    }

    public override void ClearListeners()
    {
        _priorityListeners.Clear();
    }
}
