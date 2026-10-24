
using System;
using UnityEngine;

[Serializable]
public class WaveSpawnEntry
{
    [SerializeField] private EnemyConfig _enemyConfig;
    [SerializeField, Min(0)] private int _spawnCount;
    [SerializeField, Min(0f)] private float _spawnInterval;
    [SerializeField, Min(0f)] private float _startDelay;

    public EnemyConfig EnemyConfig => _enemyConfig;
    public int SpawnCount => _spawnCount;
    public float SpawnInterval => _spawnInterval;
    public float StartDelay => _startDelay;
}
