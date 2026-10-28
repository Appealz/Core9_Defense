using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "WaveGenerationSettings", menuName = "Scriptable Objects/Wave Generation Settings")]
public sealed class WaveGenerationSettings : ScriptableObject
{
    [Header("Phase")]
    [SerializeField] private List<WaveEnemyPhaseDefinition> _enemyPhases = new();

    [Header("Patterns")]
    [SerializeField] private List<WavePatternDefinition> _patterns = new();

    [Header("Difficulty")]
    [SerializeField] private WaveDifficultySettings _difficulty = new();

    [Header("Spawn Schedule")]
    [SerializeField] private WaveSpawnScheduleSettings _spawnSchedule = new();

    [Header("Random")]
    [Tooltip("When enabled, each wave uses Random Seed + wave number and is reproducible.")]
    [SerializeField] private bool _useFixedRandomSeed;
    [SerializeField] private int _randomSeed = 9709;

    public IReadOnlyList<WaveEnemyPhaseDefinition> EnemyPhases => _enemyPhases;
    public IReadOnlyList<WavePatternDefinition> Patterns => _patterns;
    public WaveDifficultySettings Difficulty => _difficulty;
    public WaveSpawnScheduleSettings SpawnSchedule => _spawnSchedule;
    public bool UseFixedRandomSeed => _useFixedRandomSeed;
    public int RandomSeed => _randomSeed;
}

[Serializable]
public sealed class WaveEnemyPhaseDefinition
{
    [SerializeField] private WaveEnemyType _enemyType;
    [SerializeField] private EnemyConfig _enemyConfig;
    [SerializeField, Min(1)] private int _firstWave = 1;
    [Tooltip("Minimum count reserved on the first wave this enemy appears.")]
    [SerializeField, Min(0)] private int _firstWaveMinimumCount;
    [Tooltip("Zero means there is no maximum.")]
    [SerializeField, Min(0)] private int _maximumCount;
    [SerializeField] private List<WaveNumberRange> _excludedWaveRanges = new();
    [SerializeField] private List<WaveMinimumCountRule> _minimumCountRules = new();

    public WaveEnemyType EnemyType => _enemyType;
    public EnemyConfig EnemyConfig => _enemyConfig;
    public int FirstWave => Math.Max(1, _firstWave);
    public int MaximumCount => Math.Max(0, _maximumCount);

    public bool IsAvailable(int waveNumber)
    {
        if (waveNumber < FirstWave)
            return false;

        for (int i = 0; i < _excludedWaveRanges.Count; i++)
        {
            if (_excludedWaveRanges[i].Contains(waveNumber))
                return false;
        }

        return true;
    }

    public int GetMinimumCount(int waveNumber)
    {
        if (!IsAvailable(waveNumber))
            return 0;

        int minimumCount = waveNumber == FirstWave
            ? Math.Max(0, _firstWaveMinimumCount)
            : 0;

        for (int i = 0; i < _minimumCountRules.Count; i++)
        {
            WaveMinimumCountRule rule = _minimumCountRules[i];

            if (rule.Contains(waveNumber))
                minimumCount = Math.Max(minimumCount, rule.MinimumCount);
        }

        return minimumCount;
    }
}

[Serializable]
public struct WaveNumberRange
{
    [SerializeField, Min(1)] private int _firstWave;
    [SerializeField, Min(1)] private int _lastWave;

    public int FirstWave => Math.Max(1, _firstWave);
    public int LastWave => Math.Max(FirstWave, _lastWave);

    public bool Contains(int waveNumber)
    {
        return waveNumber >= FirstWave && waveNumber <= LastWave;
    }
}

[Serializable]
public struct WaveMinimumCountRule
{
    [SerializeField, Min(1)] private int _firstWave;
    [Tooltip("Zero means the rule has no final wave.")]
    [SerializeField, Min(0)] private int _lastWave;
    [SerializeField, Min(0)] private int _minimumCount;

    public int FirstWave => Math.Max(1, _firstWave);
    public int LastWave => _lastWave <= 0 ? int.MaxValue : Math.Max(FirstWave, _lastWave);
    public int MinimumCount => Math.Max(0, _minimumCount);

    public bool Contains(int waveNumber)
    {
        return waveNumber >= FirstWave && waveNumber <= LastWave;
    }
}

[Serializable]
public sealed class WavePatternDefinition
{
    [SerializeField] private WavePatternType _patternType = WavePatternType.Balanced;
    [SerializeField, Min(0f)] private float _selectionWeight = 1f;
    [Tooltip("Theme patterns are unavailable until their required enemy is available.")]
    [SerializeField] private bool _requiresEnemy;
    [SerializeField] private WaveEnemyType _requiredEnemyType;
    [SerializeField] private List<WaveEnemyWeight> _enemyWeights = new();

    public WavePatternType PatternType => _patternType;
    public float SelectionWeight => Math.Max(0f, _selectionWeight);
    public bool RequiresEnemy => _requiresEnemy;
    public WaveEnemyType RequiredEnemyType => _requiredEnemyType;
    public IReadOnlyList<WaveEnemyWeight> EnemyWeights => _enemyWeights;

    public float GetWeight(WaveEnemyType enemyType)
    {
        for (int i = 0; i < _enemyWeights.Count; i++)
        {
            if (_enemyWeights[i].EnemyType == enemyType)
                return _enemyWeights[i].Weight;
        }

        return 0f;
    }
}

[Serializable]
public struct WaveEnemyWeight
{
    [SerializeField] private WaveEnemyType _enemyType;
    [SerializeField, Min(0f)] private float _weight;

    public WaveEnemyType EnemyType => _enemyType;
    public float Weight => Math.Max(0f, _weight);
}

[Serializable]
public sealed class WaveDifficultySettings
{
    [Tooltip("Exact enemy counts beginning at Wave 1.")]
    [SerializeField] private List<int> _initialEnemyCounts = new() { 20, 21, 22, 25, 30 };
    [SerializeField, Min(1)] private int _wavesPerGrowthSegment = 5;
    [SerializeField, Min(0)] private int _enemyIncreasePerSegment = 10;
    [Tooltip("Above 1 makes growth slower near the start and faster near the end of a segment.")]
    [SerializeField, Min(0.01f)] private float _growthCurveExponent = 2f;
    [SerializeField, Min(0.01f)] private float _hpMultiplierPerWave = 1.1f;
    [SerializeField, Min(1)] private int _wavesPerDamageIncrease = 2;
    [SerializeField, Min(0.01f)] private float _damageMultiplierPerStep = 1.07f;

    public IReadOnlyList<int> InitialEnemyCounts => _initialEnemyCounts;
    public int WavesPerGrowthSegment => Math.Max(1, _wavesPerGrowthSegment);
    public int EnemyIncreasePerSegment => Math.Max(0, _enemyIncreasePerSegment);
    public float GrowthCurveExponent => Math.Max(0.01f, _growthCurveExponent);
    public float HpMultiplierPerWave => Math.Max(0.01f, _hpMultiplierPerWave);
    public int WavesPerDamageIncrease => Math.Max(1, _wavesPerDamageIncrease);
    public float DamageMultiplierPerStep => Math.Max(0.01f, _damageMultiplierPerStep);
}

[Serializable]
public sealed class WaveSpawnScheduleSettings
{
    [SerializeField, Min(2)] private int _referenceEnemyCount = 20;
    [Tooltip("Seconds from the first spawn to the last spawn at the reference count.")]
    [SerializeField, Min(0f)] private float _referenceSpawnDuration = 15f;
    [Tooltip("A different enemy is selected when this consecutive limit is reached and one is available.")]
    [SerializeField, Min(1)] private int _maximumConsecutiveSameType = 2;

    public int ReferenceEnemyCount => Math.Max(2, _referenceEnemyCount);
    public float ReferenceSpawnDuration => Math.Max(0f, _referenceSpawnDuration);
    public int MaximumConsecutiveSameType => Math.Max(1, _maximumConsecutiveSameType);
    public float SpawnInterval => ReferenceSpawnDuration / (ReferenceEnemyCount - 1);
}
