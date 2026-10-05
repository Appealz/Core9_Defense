using UnityEngine;

public interface IDamageable
{
    bool IsAlive { get; }
    Vector3 Position { get; }

    void TakeDamage(float damage);
}