using System.Collections.Generic;

public class EnemyManager
{
    private readonly EnemyFactory _enemyFactory;
    private readonly HashSet<Enemy> _aliveEnemies = new();

    public int AliveCount => _aliveEnemies.Count;
    public IReadOnlyCollection<Enemy> AliveEnemies => _aliveEnemies;

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
    }

    private void OnEnemyDied(Enemy enemy)
    {
        if (!_aliveEnemies.Remove(enemy))
            return;

        enemy.Died -= OnEnemyDied;
        _enemyFactory.Release(enemy);
    }
}