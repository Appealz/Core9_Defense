using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public sealed class ExplosionEffect : MonoBehaviour
{
    private ParticleSystem _particleSystem;
    private float _radius;

    public bool IsAlive => _particleSystem != null && _particleSystem.IsAlive(true);

    private void Awake()
    {
        _particleSystem = GetComponent<ParticleSystem>();
    }

    public void Play(Vector3 position, float radius)
    {
        transform.position = position;
        _radius = radius;

        ParticleSystem.MainModule main = _particleSystem.main;
        main.startSize = radius * 2f;

        gameObject.SetActive(true);
        _particleSystem.Play(true);
    }

    public void Clear()
    {
        _particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        transform.position = Vector3.zero;
        _radius = 0f;
    }

    private void OnDrawGizmos()
    {
        if (_radius <= 0f)
            return;

        Gizmos.DrawWireSphere(transform.position, _radius);
    }
}