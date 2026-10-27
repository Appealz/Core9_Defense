
using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class EnemyFactory
{
    private readonly ObjectPool<Enemy> _enemyPool;
    private readonly ProjectileManager _projectileManager;
    private readonly CoreBoundary _coreBoundary;
    private readonly CameraBoundsProvider _cameraBoundsProvider;

    public EnemyFactory(
        ObjectPool<Enemy> enemyPool,
        ProjectileManager projectileManager,
        CoreBoundary coreBoundary,
        CameraBoundsProvider cameraBoundsProvider)
    {
        _enemyPool = enemyPool ?? throw new ArgumentNullException(nameof(enemyPool));
        _projectileManager = projectileManager ?? throw new ArgumentNullException(nameof(projectileManager));
        _coreBoundary = coreBoundary != null ? coreBoundary : throw new ArgumentNullException(nameof(coreBoundary));
        _cameraBoundsProvider = cameraBoundsProvider ?? throw new ArgumentNullException(nameof(cameraBoundsProvider));
    }

    public async UniTask<Enemy> CreateAsync(EnemyConfig config, Transform moveTarget)
    {
        if (config == null)
            throw new ArgumentNullException(nameof(config));

        if (moveTarget == null)
            throw new ArgumentNullException(nameof(moveTarget));

        if (config.MovementDefinition == null)
            throw new InvalidOperationException("MovementDefinition이 설정되지 않았습니다.");

        Enemy enemy = await _enemyPool.GetAsync();

        EnemyHealth health = new EnemyHealth(config.MaxHp);
        EnemyStats stats = new EnemyStats(config.MoveSpeed, config.AttackDamage, config.AttackRange, config.AttackCooldown);

        IEnemyMovement movement = config.MovementDefinition.CreateMovement();
        IEnemyAttack attack = config.AttackDefinition?.CreateAttack(enemy.transform, _projectileManager, _coreBoundary);

        EnemyController controller = new EnemyController(stats, movement, attack, _coreBoundary);

        enemy.Initialize(health, stats, controller, attack, moveTarget, config, _cameraBoundsProvider);

        return enemy;
    }

    public void Release(Enemy enemy)
    {
        if (enemy == null)
            return;

        _enemyPool.Return(enemy);
    }
}
