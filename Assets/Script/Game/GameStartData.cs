using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GameStartData", menuName = "Scriptable Objects/GameStartData")]
public class GameStartData : ScriptableObject
{
    [SerializeField] private int _initialTowerSlotNumber = 5;
    [SerializeField] private TowerPartData _basicTowerPartData;
    [SerializeField] private List<WaveData> _waveDataList = new();
    [SerializeField] private List<RewardData> _rewardDataList = new();
    [SerializeField] private List<TowerTypeUpgradeData> _towerTypeUpgradeDataList = new();
    [SerializeField] private List<CoreGoldUpgradeData> _coreGoldUpgradeDataList = new();
    [SerializeField] private List<TowerGoldUpgradeData> _towerGoldUpgradeDataList = new();
    [SerializeField] private RewardSelectionSettings _rewardSelectionSettings = new();
    [SerializeField] private PlayerLevelData _playerLevelData;

    public int InitialTowerSlotNumber => _initialTowerSlotNumber;
    public TowerPartData BasicTowerPartData => _basicTowerPartData;
    public IReadOnlyList<WaveData> WaveDataList => _waveDataList;
    public IReadOnlyList<RewardData> RewardDataList => _rewardDataList;
    public IReadOnlyList<TowerTypeUpgradeData> TowerTypeUpgradeDataList => _towerTypeUpgradeDataList;
    public IReadOnlyList<CoreGoldUpgradeData> CoreGoldUpgradeDataList => _coreGoldUpgradeDataList;
    public IReadOnlyList<TowerGoldUpgradeData> TowerGoldUpgradeDataList => _towerGoldUpgradeDataList;
    public RewardSelectionSettings RewardSelectionSettings => _rewardSelectionSettings;
    public PlayerLevelData PlayerLevelData => _playerLevelData;
}
