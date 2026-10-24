
using UnityEngine;

public readonly struct EnemyDeathContext
{
    public EnemyConfig SourceConfig { get; }
    public Vector3 Position { get; }
    public SpawnManager SpawnManager { get; }
    public EnemyManager EnemyManager { get; }

    public EnemyDeathContext(EnemyConfig sourceConfig, Vector3 position, SpawnManager spawnManager, EnemyManager enemyManager)
    {
        SourceConfig = sourceConfig;
        Position = position;
        SpawnManager = spawnManager;
        EnemyManager = enemyManager;
    }
}
