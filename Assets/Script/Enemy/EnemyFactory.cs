using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class EnemyFactory
{
    private readonly ObjectPool<Enemy> _enemyPool;

    public EnemyFactory(ObjectPool<Enemy> enemyPool)
    {
        _enemyPool = enemyPool ?? throw new ArgumentNullException(nameof(enemyPool));
    }

    public async UniTask<Enemy> CreateAsync(EnemyConfig config, Transform moveTarget)
    {
        if (config == null)
            throw new ArgumentNullException(nameof(config));

        if (moveTarget == null)
            throw new ArgumentNullException(nameof(moveTarget));

        if (config.MovementDefinition == null)
            throw new InvalidOperationException("MovementDefinition이 설정되지 않았습니다.");

        if (config.AttackDefinition == null)
            throw new InvalidOperationException("AttackDefinition이 설정되지 않았습니다.");

        Enemy enemy = await _enemyPool.GetAsync();

        EnemyHealth health = new EnemyHealth(config.MaxHp);
        EnemyStats stats = new EnemyStats(config.MoveSpeed, config.AttackDamage, config.AttackRange, config.AttackCooldown);

        IEnemyMovement movement = config.MovementDefinition.CreateMovement();
        IEnemyAttack attack = config.AttackDefinition.CreateAttack();

        EnemyController controller = new EnemyController(stats, movement);

        enemy.Initialize(health, stats, controller, attack, moveTarget, config.Sprite);

        return enemy;
    }

    public void Release(Enemy enemy)
    {
        if (enemy == null)
            return;

        _enemyPool.Return(enemy);
    }
}