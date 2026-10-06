using UnityEngine;

public sealed class DirectHitImpact : IProjectileImpact
{
    public void Impact(Vector3 position, IDamageable target, float damage)
    {
        if (!IsTargetValid(target))
            return;

        target.TakeDamage(damage);
    }

    private bool IsTargetValid(IDamageable target)
    {
        if (target == null)
            return false;

        if (target is Object unityObject && unityObject == null)
            return false;

        return target.IsAlive;
    }
}