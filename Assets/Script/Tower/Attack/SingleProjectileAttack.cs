using Cysharp.Threading.Tasks;
using UnityEngine;

public class SingleProjectileAttack : ITargetedTowerAttack
{
    private readonly ProjectileManager _projectileManager;
    private readonly IProjectileImpact _impact;
    private readonly float _projectileSpeed;

    public SingleProjectileAttack(ProjectileManager projectileManager, float projectileSpeed)
    {
        _projectileManager = projectileManager;
        _projectileSpeed = projectileSpeed;

        _impact = new DirectHitImpact();
    }

    public void Attack(Vector3 origin, Enemy target, TowerStats stats)
    {
        if (target == null || !target.IsAlive)
            return;

        _projectileManager.FireAsync(origin, target, stats.AttackDamage, _projectileSpeed, _impact).Forget();
    }
}