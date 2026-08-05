using UnityEngine;

public sealed class UnityLifecycleRunner : MonoBehaviour
{
    private LifecycleCore _lifecycleCore;

    private void Awake()
    {
        _lifecycleCore = new LifecycleCore();
    }

    private void Update()
    {
        _lifecycleCore.Update(Time.deltaTime);
    }

    public void Register(IUpdatable updatable)
    {
        _lifecycleCore.Register(updatable);
    }

    public void Unregister(IUpdatable updatable)
    {
        _lifecycleCore.Unregister(updatable);
    }
}