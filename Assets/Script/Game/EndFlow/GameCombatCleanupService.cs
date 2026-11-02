using System;

public sealed class GameCombatCleanupService
{
    private readonly EnemyManager _enemyManager;
    private readonly ProjectileManager _projectileManager;
    private readonly LaserManager _laserManager;
    private readonly ExplosionEffectManager _explosionEffectManager;

    public GameCombatCleanupService(
        EnemyManager enemyManager,
        ProjectileManager projectileManager,
        LaserManager laserManager,
        ExplosionEffectManager explosionEffectManager)
    {
        _enemyManager = enemyManager ?? throw new ArgumentNullException(nameof(enemyManager));
        _projectileManager = projectileManager ?? throw new ArgumentNullException(nameof(projectileManager));
        _laserManager = laserManager ?? throw new ArgumentNullException(nameof(laserManager));
        _explosionEffectManager = explosionEffectManager ??
            throw new ArgumentNullException(nameof(explosionEffectManager));
    }

    public void ClearForWaveRestart()
    {
        _enemyManager.ClearAll();
        _projectileManager.ClearAll();
        _laserManager.ClearAll();
        _explosionEffectManager.ClearAll();
    }
}
