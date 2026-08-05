using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class LifecycleCore
{
    private readonly List<IUpdatable> _updatables = new();

    public void Register(IUpdatable updatable)
    {
        if (updatable == null)
        {
            throw new ArgumentNullException(nameof(updatable));
        }

        if (_updatables.Contains(updatable))
        {
            return;
        }

        _updatables.Add(updatable);
    }

    public void Unregister(IUpdatable updatable)
    {
        if (updatable == null)
        {
            return;
        }

        _updatables.Remove(updatable);
    }

    public void Update(float deltaTime)
    {
        for (int i = 0; i < _updatables.Count; i++)
        {
            _updatables[i].Update(deltaTime);
        }
    }
}
