
using System;

public sealed class EnemyDeathBehaviorService : IDisposable
{
    private readonly EnemyManager _enemyManager;
    private readonly SpawnManager _spawnManager;

    public EnemyDeathBehaviorService(EnemyManager enemyManager, SpawnManager spawnManager)
    {
        _enemyManager = enemyManager ?? throw new ArgumentNullException(nameof(enemyManager));
        _spawnManager = spawnManager ?? throw new ArgumentNullException(nameof(spawnManager));

        _enemyManager.EnemyKilled += HandleEnemyKilled;
    }

    private void HandleEnemyKilled(Enemy enemy)
    {
        EnemyConfig config = enemy.Config;

        if (config == null || config.DeathBehaviorDefinition == null)
            return;

        EnemyDeathContext context = new EnemyDeathContext(
            config,
            enemy.RuntimeModifiers,
            _spawnManager.WaveCancellationToken,
            enemy.Position,
            _spawnManager,
            _enemyManager);
        config.DeathBehaviorDefinition.Execute(context);
    }

    public void Dispose()
    {
        _enemyManager.EnemyKilled -= HandleEnemyKilled;
    }
}
