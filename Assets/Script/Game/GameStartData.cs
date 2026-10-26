using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GameStartData", menuName = "Scriptable Objects/GameStartData")]
public class GameStartData : ScriptableObject
{
    [SerializeField] private int _initialTowerSlotNumber = 5;
    [SerializeField] private TowerPartData _basicTowerPartData;
    [SerializeField] private List<WaveData> _waveDataList = new();
    [SerializeField] private List<RewardData> _rewardDataList = new();
    [SerializeField] private RewardSelectionSettings _rewardSelectionSettings = new();

    public int InitialTowerSlotNumber => _initialTowerSlotNumber;
    public TowerPartData BasicTowerPartData => _basicTowerPartData;
    public IReadOnlyList<WaveData> WaveDataList => _waveDataList;
    public IReadOnlyList<RewardData> RewardDataList => _rewardDataList;
    public RewardSelectionSettings RewardSelectionSettings => _rewardSelectionSettings;
}
