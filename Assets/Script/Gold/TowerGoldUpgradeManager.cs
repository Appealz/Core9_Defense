using System;
using System.Collections.Generic;

public sealed class TowerGoldUpgradeManager : IDisposable
{
    private readonly GoldManager _goldManager;
    private readonly GameStateManager _gameStateManager;
    private readonly ITowerGoldUpgradeApplier _upgradeApplier;
    private readonly List<TowerGoldUpgradeData> _upgradeData = new();
    private readonly Dictionary<Tower, Dictionary<TowerGoldUpgradeData, int>> _towerLevels = new();
    private readonly List<string> _configurationErrors = new();
    private bool _isPurchasing;

    public event Action<Tower, TowerGoldUpgradeData, int> UpgradeLevelChanged;
    public event Action<Tower> TowerUpgradeStateRemoved;

    public IReadOnlyList<TowerGoldUpgradeData> UpgradeData => _upgradeData;
    public IReadOnlyList<string> ConfigurationErrors => _configurationErrors;

    public TowerGoldUpgradeManager(
        GoldManager goldManager,
        GameStateManager gameStateManager,
        ITowerGoldUpgradeApplier upgradeApplier,
        IReadOnlyList<TowerGoldUpgradeData> upgradeData)
    {
        _goldManager = goldManager ?? throw new ArgumentNullException(nameof(goldManager));
        _gameStateManager = gameStateManager ?? throw new ArgumentNullException(nameof(gameStateManager));
        _upgradeApplier = upgradeApplier ?? throw new ArgumentNullException(nameof(upgradeApplier));

        RegisterUpgradeData(upgradeData);
        _upgradeApplier.TowerRemoved += HandleTowerRemoved;
    }

    public int GetLevel(Tower tower, TowerGoldUpgradeData data)
    {
        if (tower == null || data == null ||
            !_towerLevels.TryGetValue(tower, out Dictionary<TowerGoldUpgradeData, int> levels))
        {
            return 0;
        }

        return levels.TryGetValue(data, out int level) ? level : 0;
    }

    public float GetTotalFlatIncrease(Tower tower, TowerGoldUpgradeData data) =>
        data == null ? 0f : data.FlatIncreasePerLevel * GetLevel(tower, data);

    public TowerGoldUpgradePurchaseResult GetPurchaseResult(
        Tower tower,
        TowerGoldUpgradeData data,
        out int nextCost)
    {
        nextCost = 0;

        if (data == null || !_upgradeData.Contains(data))
            return TowerGoldUpgradePurchaseResult.InvalidData;

        if (_isPurchasing)
            return TowerGoldUpgradePurchaseResult.PurchaseInProgress;

        if (_gameStateManager.CurrentState != GameState.Playing)
            return TowerGoldUpgradePurchaseResult.NotPlaying;

        if (tower == null || !tower.IsAlive || !_upgradeApplier.IsTowerActive(tower))
            return TowerGoldUpgradePurchaseResult.TowerNotAvailable;

        int level = GetLevel(tower, data);

        if (data.IsMaxLevelReached(level))
            return TowerGoldUpgradePurchaseResult.MaxLevel;

        if (!data.TryGetNextCost(level, out nextCost) || nextCost <= 0)
            return TowerGoldUpgradePurchaseResult.InvalidCost;

        if (!_upgradeApplier.CanApplyTowerGoldUpgrade(tower, data.StatType, data.FlatIncreasePerLevel))
            return TowerGoldUpgradePurchaseResult.InvalidData;

        if (_goldManager.CurrentGold < nextCost)
            return TowerGoldUpgradePurchaseResult.InsufficientGold;

        return TowerGoldUpgradePurchaseResult.Success;
    }

    public TowerGoldUpgradePurchaseResult TryPurchase(Tower tower, TowerGoldUpgradeData data)
    {
        TowerGoldUpgradePurchaseResult validation = GetPurchaseResult(tower, data, out int cost);

        if (validation != TowerGoldUpgradePurchaseResult.Success)
            return validation;

        _isPurchasing = true;
        bool appliedUpgrade = false;
        int goldBeforePurchase = _goldManager.CurrentGold;
        int purchasedLevel = 0;
        TowerGoldUpgradePurchaseResult result;

        try
        {
            if (!_goldManager.TrySpendGold(cost))
            {
                result = TowerGoldUpgradePurchaseResult.InsufficientGold;
            }
            else if (!_upgradeApplier.TryApplyTowerGoldUpgrade(
                tower,
                data.StatType,
                data.FlatIncreasePerLevel))
            {
                RestoreGold(goldBeforePurchase);
                result = TowerGoldUpgradePurchaseResult.ApplyFailed;
            }
            else
            {
                appliedUpgrade = true;
                Dictionary<TowerGoldUpgradeData, int> levels = GetOrCreateLevels(tower);
                purchasedLevel = GetLevel(tower, data) + 1;
                levels[data] = purchasedLevel;
                result = TowerGoldUpgradePurchaseResult.Success;
            }
        }
        catch
        {
            if (!appliedUpgrade)
                RestoreGold(goldBeforePurchase);

            result = TowerGoldUpgradePurchaseResult.ApplyFailed;
        }
        finally
        {
            _isPurchasing = false;
        }

        if (result == TowerGoldUpgradePurchaseResult.Success)
            UpgradeLevelChanged?.Invoke(tower, data, purchasedLevel);

        return result;
    }

    public void Dispose()
    {
        _upgradeApplier.TowerRemoved -= HandleTowerRemoved;
        _towerLevels.Clear();
    }

    private Dictionary<TowerGoldUpgradeData, int> GetOrCreateLevels(Tower tower)
    {
        if (_towerLevels.TryGetValue(tower, out Dictionary<TowerGoldUpgradeData, int> levels))
            return levels;

        levels = new Dictionary<TowerGoldUpgradeData, int>();
        _towerLevels.Add(tower, levels);
        return levels;
    }

    private void HandleTowerRemoved(Tower tower)
    {
        if (tower == null || !_towerLevels.Remove(tower))
            return;

        TowerUpgradeStateRemoved?.Invoke(tower);
    }

    private void RestoreGold(int targetAmount)
    {
        if (_goldManager.CurrentGold >= targetAmount)
            return;

        try
        {
            _goldManager.AddGold(targetAmount - _goldManager.CurrentGold);
        }
        catch
        {
            // GoldManager commits its balance before publishing GoldChanged.
        }
    }

    private void RegisterUpgradeData(IReadOnlyList<TowerGoldUpgradeData> upgradeData)
    {
        if (upgradeData == null)
        {
            _configurationErrors.Add("Tower gold upgrade data list is null.");
            return;
        }

        var registeredIds = new HashSet<string>(StringComparer.Ordinal);

        for (int i = 0; i < upgradeData.Count; i++)
        {
            TowerGoldUpgradeData data = upgradeData[i];

            if (data == null)
            {
                _configurationErrors.Add($"Tower gold upgrade data at index {i} is null.");
                continue;
            }

            if (!data.IsConfigurationValid(out string error))
            {
                _configurationErrors.Add(error);
                continue;
            }

            if (!registeredIds.Add(data.UpgradeId))
            {
                _configurationErrors.Add($"Duplicate tower gold upgrade id: '{data.UpgradeId}'.");
                continue;
            }

            _upgradeData.Add(data);
        }
    }
}
