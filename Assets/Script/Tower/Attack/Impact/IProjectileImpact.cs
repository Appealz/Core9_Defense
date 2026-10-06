using UnityEngine;

public interface IProjectileImpact
{
    void Impact(Vector3 position, IDamageable target, float damage);
}