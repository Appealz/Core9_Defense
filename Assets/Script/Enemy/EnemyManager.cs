
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
    public int CombatAliveCount => CountAliveByRole(EnemyRole.Combat);
    public int SupportAliveCount => CountAliveByRole(EnemyRole.Support);
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
        enemy.Retreated += OnEnemyRetreated;
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

    private void OnEnemyRetreated(Enemy enemy)
    {
        RemoveEnemy(enemy, false);
    }

    public void SetSupportRetreatWaiting(bool shouldWait)
    {
        List<Enemy> supportEnemies = new();

        foreach (Enemy enemy in _aliveEnemies)
        {
            if (enemy != null && enemy.IsAlive && enemy.Config != null && enemy.Config.Role == EnemyRole.Support)
                supportEnemies.Add(enemy);
        }

        for (int i = 0; i < supportEnemies.Count; i++)
            supportEnemies[i].SetRetreatWaiting(shouldWait);
    }

    private void RemoveEnemy(Enemy enemy, bool wasKilled)
    {
        if (!_aliveEnemies.Remove(enemy))
            return;

        enemy.Died -= OnEnemyDied;
        enemy.CoreReached -= OnEnemyCoreReached;
        enemy.Retreated -= OnEnemyRetreated;

        if (wasKilled)
            EnemyKilled?.Invoke(enemy);

        _enemyFactory.Release(enemy);
        EnemyRemoved?.Invoke();
    }

    private int CountAliveByRole(EnemyRole role)
    {
        int count = 0;

        foreach (Enemy enemy in _aliveEnemies)
        {
            if (enemy != null && enemy.IsAlive && enemy.Config != null && enemy.Config.Role == role)
                count++;
        }

        return count;
    }
}
