using System;

public sealed class RunScoreCounter : IDisposable
{
    private readonly EnemyManager _enemyManager;
    private readonly GameEndRewardPolicy _rewardPolicy;
    private int _defeatedEnemyCount;

    public int DefeatedEnemyCount => _defeatedEnemyCount;
    public int Score => _rewardPolicy.CalculateScore(_defeatedEnemyCount);

    public RunScoreCounter(EnemyManager enemyManager, GameEndRewardPolicy rewardPolicy)
    {
        _enemyManager = enemyManager ?? throw new ArgumentNullException(nameof(enemyManager));
        _rewardPolicy = rewardPolicy ?? throw new ArgumentNullException(nameof(rewardPolicy));
        _enemyManager.EnemyKilled += HandleEnemyKilled;
    }

    private void HandleEnemyKilled(Enemy enemy)
    {
        if (_defeatedEnemyCount < int.MaxValue)
            _defeatedEnemyCount++;
    }

    public void Dispose()
    {
        _enemyManager.EnemyKilled -= HandleEnemyKilled;
    }
}
