using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EventChannel", menuName = "EventChannel/Void")]
public class VoidEventChannelSO : EventChannelBase
{
    private readonly SortedList<int, List<Action>> _priorityListeners = new SortedList<int, List<Action>>();

    public void AddListener(Action action, int priority = 0)
    {
        if (!_priorityListeners.ContainsKey(priority))
        {
            _priorityListeners.Add(priority, new List<Action>());
        }
        _priorityListeners[priority].Add(action);
    }

    public void RemoveListener(Action action)
    {
        foreach (var list in _priorityListeners.Values)
        {
            if (list.Contains(action))
            {
                list.Remove(action);
            }
        }
    }

    public void RaiseEvent()
    {
        foreach (var list in _priorityListeners.Values)
        {
            foreach (var listener in list)
            {
                listener?.Invoke();
            }
        }
    }

    public override void ClearListeners()
    {
        _priorityListeners.Clear();
    }
}
