using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "WaveData", menuName = "Scriptable Objects/WaveData")]
public class WaveData : ScriptableObject
{
    [SerializeField] private List<WaveSpawnEntry> _spawnEntries = new();

    public IReadOnlyList<WaveSpawnEntry> SpawnEntries => _spawnEntries;
}