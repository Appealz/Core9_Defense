
using System.Threading;
using UnityEngine;

public readonly struct EnemyDeathContext
{
    public EnemyConfig SourceConfig { get; }
    public EnemyRuntimeModifiers RuntimeModifiers { get; }
    public CancellationToken CancellationToken { get; }
    public Vector3 Position { get; }
    public SpawnManager SpawnManager { get; }
    public EnemyManager EnemyManager { get; }

    public EnemyDeathContext(
        EnemyConfig sourceConfig,
        EnemyRuntimeModifiers runtimeModifiers,
        CancellationToken cancellationToken,
        Vector3 position,
        SpawnManager spawnManager,
        EnemyManager enemyManager)
    {
        SourceConfig = sourceConfig;
        RuntimeModifiers = runtimeModifiers;
        CancellationToken = cancellationToken;
        Position = position;
        SpawnManager = spawnManager;
        EnemyManager = enemyManager;
    }
}
