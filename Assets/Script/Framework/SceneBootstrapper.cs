using System;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public abstract class SceneBootstrapper : MonoBehaviour
{
    [SerializeField]
    private UnityLifecycleRunner _lifecycleRunner;

    private readonly List<IUpdatable> _registeredUpdatables = new();

    private bool _isReady;

    private void Awake()
    {
        if (_lifecycleRunner == null)
        {
            Debug.LogError($"{GetType().Name}: " + $"{nameof(UnityLifecycleRunner)} 참조가 설정되지 않았습니다.",this);
            return;
        }
        _isReady = true;
    }

    private void Start()
    {
        if (!_isReady)
            return;
        Compose();
    }

    protected abstract void Compose();

    protected void Register(IUpdatable updatable)
    {
        if (updatable == null)
            throw new ArgumentNullException(nameof(updatable));

        if (!_isReady)
        {
            throw new InvalidOperationException($"{nameof(SceneBootstrapper)}가 준비되지 않았습니다.");
        }

        if (_registeredUpdatables.Contains(updatable))
            return;

        _lifecycleRunner.Register(updatable);
        _registeredUpdatables.Add(updatable);
    }

    private void OnDestroy()
    {
        if (!_isReady)
            return;

        for (int i = _registeredUpdatables.Count - 1; i >= 0; i--)
        {
            _lifecycleRunner.Unregister(_registeredUpdatables[i]);
        }

        _registeredUpdatables.Clear();
    }
}