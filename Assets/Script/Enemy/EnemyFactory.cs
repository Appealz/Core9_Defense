using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class EnemyFactory
{
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

        if (string.IsNullOrWhiteSpace(config.PrefabKey))
            throw new InvalidOperationException($"{config.name}: PrefabKey가 설정되지 않았습니다.");

        EnemyHealth health = new EnemyHealth(config.MaxHp);
        EnemyStats stats = new EnemyStats(config.MoveSpeed, config.AttackDamage, config.AttackRange, config.AttackCooldown);

        IEnemyMovement movement = config.MovementDefinition.CreateMovement();
        IEnemyAttack attack = config.AttackDefinition.CreateAttack();

        EnemyController controller = new EnemyController(stats, movement);

        GameObject enemyObject = await Addressables.InstantiateAsync(config.PrefabKey).Task;
        Enemy enemy = enemyObject.GetComponent<Enemy>();

        if (enemy == null)
        {
            Addressables.ReleaseInstance(enemyObject);
            throw new InvalidOperationException($"{config.PrefabKey} Prefab에 Enemy 컴포넌트가 없습니다.");
        }

        enemy.Initialize(health, stats, controller, attack, moveTarget);

        return enemy;
    }

    public void Release(Enemy enemy)
    {
        if (enemy == null)
            return;

        Addressables.ReleaseInstance(enemy.gameObject);
    }
}