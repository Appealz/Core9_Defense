
using System;

public sealed class EnemyGoldRewardService : IDisposable
{
    private readonly EnemyManager _enemyManager;
    private readonly GoldManager _goldManager;

    public EnemyGoldRewardService(EnemyManager enemyManager, GoldManager goldManager)
    {
        _enemyManager = enemyManager;
        _goldManager = goldManager;

        _enemyManager.EnemyKilled += HandleEnemyKilled;
    }

    private void HandleEnemyKilled(Enemy enemy)
    {
        _goldManager.AddGold(enemy.GoldReward);
    }

    public void Dispose()
    {
        _enemyManager.EnemyKilled -= HandleEnemyKilled;
    }
}
