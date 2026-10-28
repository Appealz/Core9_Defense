using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

public readonly struct EnemyRuntimeModifiers
{
    public static EnemyRuntimeModifiers Default => new(1f, 1f);

    public float HpMultiplier { get; }
    public float DamageMultiplier { get; }

    public EnemyRuntimeModifiers(float hpMultiplier, float damageMultiplier)
    {
        if (hpMultiplier <= 0f)
            throw new ArgumentOutOfRangeException(nameof(hpMultiplier));

        if (damageMultiplier <= 0f)
            throw new ArgumentOutOfRangeException(nameof(damageMultiplier));

        HpMultiplier = hpMultiplier;
        DamageMultiplier = damageMultiplier;
    }
}

public readonly struct WaveSpawnInstruction
{
    public WaveEnemyType EnemyType { get; }
    public EnemyConfig EnemyConfig { get; }
    public float SpawnTime { get; }

    public WaveSpawnInstruction(WaveEnemyType enemyType, EnemyConfig enemyConfig, float spawnTime)
    {
        EnemyType = enemyType;
        EnemyConfig = enemyConfig != null
            ? enemyConfig
            : throw new ArgumentNullException(nameof(enemyConfig));
        SpawnTime = Math.Max(0f, spawnTime);
    }
}

public sealed class WavePlan
{
    public int WaveNumber { get; }
    public WavePatternType PatternType { get; }
    public EnemyRuntimeModifiers RuntimeModifiers { get; }
    public IReadOnlyDictionary<WaveEnemyType, int> EnemyCounts { get; }
    public IReadOnlyList<WaveSpawnInstruction> SpawnInstructions { get; }
    public int TotalEnemyCount => SpawnInstructions.Count;
    public float SpawnDuration => SpawnInstructions.Count > 0
        ? SpawnInstructions[SpawnInstructions.Count - 1].SpawnTime
        : 0f;

    public WavePlan(
        int waveNumber,
        WavePatternType patternType,
        EnemyRuntimeModifiers runtimeModifiers,
        IDictionary<WaveEnemyType, int> enemyCounts,
        IList<WaveSpawnInstruction> spawnInstructions)
    {
        WaveNumber = waveNumber;
        PatternType = patternType;
        RuntimeModifiers = runtimeModifiers;
        EnemyCounts = new ReadOnlyDictionary<WaveEnemyType, int>(
            new Dictionary<WaveEnemyType, int>(enemyCounts));
        SpawnInstructions = new ReadOnlyCollection<WaveSpawnInstruction>(
            new List<WaveSpawnInstruction>(spawnInstructions));
    }
}
