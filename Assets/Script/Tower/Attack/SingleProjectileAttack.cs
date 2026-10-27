using Cysharp.Threading.Tasks;
using UnityEngine;

public class SingleProjectileAttack : ITargetedTowerAttack
{
    private readonly ProjectileManager _projectileManager;
    private readonly IProjectileImpact _impact;
    private readonly float _projectileSpeed;
    private readonly TowerUniqueModifiers _uniqueModifiers;

    public SingleProjectileAttack(
        ProjectileManager projectileManager,
        float projectileSpeed,
        TowerUniqueModifiers uniqueModifiers)
    {
        _projectileManager = projectileManager;
        _projectileSpeed = projectileSpeed;
        _uniqueModifiers = uniqueModifiers;

        _impact = new DirectHitImpact();
    }

    public void Attack(Vector3 origin, Enemy target, TowerStats stats, float damage)
    {
        if (target == null || !target.IsAlive)
            return;

        int projectileCount = _uniqueModifiers.CalculateInt(TowerUniqueStatType.ProjectileCount, 1);
        float spreadAngle = _uniqueModifiers.Calculate(TowerUniqueStatType.SpreadAngle, 0f);
        float spacing = _uniqueModifiers.Calculate(TowerUniqueStatType.ProjectileSpacing, 0f);
        Vector3 direction = (target.Position - origin).normalized;
        Vector3 perpendicular = new Vector3(-direction.y, direction.x, 0f);

        for (int i = 0; i < projectileCount; i++)
        {
            float centeredIndex = i - (projectileCount - 1) * 0.5f;
            Vector3 shotOrigin = origin + perpendicular * (centeredIndex * spacing);

            if (spreadAngle <= 0f)
            {
                _projectileManager.FireAsync(
                    shotOrigin,
                    target,
                    damage,
                    _projectileSpeed,
                    _impact).Forget();
                continue;
            }

            Vector3 shotDirection = Quaternion.Euler(0f, 0f, centeredIndex * spreadAngle) * direction;
            Vector3 targetPosition = shotOrigin + shotDirection * stats.AttackRange;
            _projectileManager.FireFixedAsync(
                shotOrigin,
                targetPosition,
                damage,
                _projectileSpeed,
                _impact).Forget();
        }
    }
}
