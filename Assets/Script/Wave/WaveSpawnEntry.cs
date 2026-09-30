using System;
using UnityEngine;

[Serializable]
public class WaveSpawnEntry
{
    [SerializeField] private EnemyConfig _enemyConfig;
    [SerializeField] private int _spawnCount;
    [SerializeField] private float _spawnInterval;

    public EnemyConfig EnemyConfig => _enemyConfig;
    public int SpawnCount => _spawnCount;
    public float SpawnInterval => _spawnInterval;
}