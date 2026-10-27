using System;

public sealed class EnemyExperienceRewardService : IDisposable
{
    private readonly EnemyManager _enemyManager;
    private readonly PlayerProgression _playerProgression;

    public EnemyExperienceRewardService(EnemyManager enemyManager, PlayerProgression playerProgression)
    {
        _enemyManager = enemyManager ?? throw new ArgumentNullException(nameof(enemyManager));
        _playerProgression = playerProgression ?? throw new ArgumentNullException(nameof(playerProgression));
        _enemyManager.EnemyKilled += HandleEnemyKilled;
    }

    private void HandleEnemyKilled(Enemy enemy)
    {
        if (enemy?.Config != null)
            _playerProgression.AddExperience(enemy.Config.ExperienceReward);
    }

    public void Dispose()
    {
        _enemyManager.EnemyKilled -= HandleEnemyKilled;
    }
}
