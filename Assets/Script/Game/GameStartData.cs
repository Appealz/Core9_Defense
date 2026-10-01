using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GameStartData", menuName = "Scriptable Objects/GameStartData")]
public class GameStartData : ScriptableObject
{
    [SerializeField] private int _initialTowerSlotNumber = 5;
    [SerializeField] private TowerPartData _basicTowerPartData;
    [SerializeField] private List<WaveData> _waveDataList = new();

    public int InitialTowerSlotNumber => _initialTowerSlotNumber;
    public TowerPartData BasicTowerPartData => _basicTowerPartData;
    public IReadOnlyList<WaveData> WaveDataList => _waveDataList;
}