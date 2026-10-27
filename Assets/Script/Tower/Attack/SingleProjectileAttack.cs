using Cysharp.Threading.Tasks;
using UnityEngine;

public class SingleProjectileAttack : ITargetedTowerAttack
{
    private readonly ProjectileManager _projectileManager;
    private readonly IProjectileImpact _impact;
    private readonly float _projectileSpeed;
    private readonly float _straightProjectileLifetime;
    private readonly TowerUniqueModifiers _uniqueModifiers;

    public SingleProjectileAttack(
        ProjectileManager projectileManager,
        float projectileSpeed,
        float straightProjectileLifetime,
        TowerUniqueModifiers uniqueModifiers)
    {
        _projectileManager = projectileManager;
        _projectileSpeed = projectileSpeed;
        _straightProjectileLifetime = Mathf.Max(0.01f, straightProjectileLifetime);
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

            bool useStraightProjectile = projectileCount > 1 || spreadAngle > 0f || spacing > 0f;

            if (!useStraightProjectile)
            {
                _projectileManager.FireAsync(
                    shotOrigin,
                    target,
                    damage,
                    _projectileSpeed,
                    _impact).Forget();
                continue;
            }

            float angle = GetSpreadAngle(i, spreadAngle);
            Vector3 shotDirection = Quaternion.Euler(0f, 0f, angle) * direction;
            _projectileManager.FireFixedAsync(
                shotOrigin,
                shotDirection,
                stats.AttackRange,
                _straightProjectileLifetime,
                damage,
                _projectileSpeed,
                _impact,
                ProjectileCollisionTarget.Enemy).Forget();
        }
    }

    private static float GetSpreadAngle(int projectileIndex, float spreadAngle)
    {
        if (projectileIndex <= 0 || spreadAngle <= 0f)
            return 0f;

        int step = (projectileIndex + 1) / 2;
        return step * spreadAngle * (projectileIndex % 2 == 1 ? 1f : -1f);
    }
}
