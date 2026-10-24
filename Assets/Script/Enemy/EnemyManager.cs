
using System;
using System.Collections.Generic;

public class EnemyManager
{
    public event Action EnemyRemoved;
    public event Action<Enemy> EnemyKilled;
    public event Action<int> SpawnCountAdjusted;

    private readonly EnemyFactory _enemyFactory;
    private readonly HashSet<Enemy> _aliveEnemies = new();

    public int AliveCount => _aliveEnemies.Count;
    public int PendingSpawnCount { get; private set; }

    internal IReadOnlyCollection<Enemy> AliveEnemies => _aliveEnemies;

    public EnemyManager(EnemyFactory enemyFactory)
    {
        _enemyFactory = enemyFactory;
    }

    public void Register(Enemy enemy)
    {
        if (enemy == null)
            return;

        if (!_aliveEnemies.Add(enemy))
            return;

        enemy.Died += OnEnemyDied;
        enemy.CoreReached += OnEnemyCoreReached;
    }

    public void ReserveAdditionalSpawns(int count)
    {
        if (count <= 0)
            return;

        PendingSpawnCount += count;
        SpawnCountAdjusted?.Invoke(count);
    }

    public void CompleteReservedSpawn(bool succeeded)
    {
        if (PendingSpawnCount <= 0)
            throw new InvalidOperationException("예약된 Enemy 스폰이 없습니다.");

        PendingSpawnCount--;

        if (!succeeded)
            SpawnCountAdjusted?.Invoke(-1);
    }

    private void OnEnemyDied(Enemy enemy)
    {
        RemoveEnemy(enemy, true);
    }

    private void OnEnemyCoreReached(Enemy enemy)
    {
        RemoveEnemy(enemy, false);
    }

    private void RemoveEnemy(Enemy enemy, bool wasKilled)
    {
        if (!_aliveEnemies.Remove(enemy))
            return;

        enemy.Died -= OnEnemyDied;
        enemy.CoreReached -= OnEnemyCoreReached;

        if (wasKilled)
            EnemyKilled?.Invoke(enemy);

        _enemyFactory.Release(enemy);
        EnemyRemoved?.Invoke();
    }
}
