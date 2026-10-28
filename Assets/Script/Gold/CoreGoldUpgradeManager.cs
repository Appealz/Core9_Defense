using System;
using System.Collections.Generic;

public sealed class CoreGoldUpgradeManager
{
    private readonly GoldManager _goldManager;
    private readonly GameStateManager _gameStateManager;
    private readonly ICoreGoldUpgradeApplier _upgradeApplier;
    private readonly List<CoreGoldUpgradeData> _upgradeData = new();
    private readonly Dictionary<CoreGoldUpgradeData, int> _levels = new();
    private readonly List<string> _configurationErrors = new();
    private bool _isPurchasing;

    public event Action<CoreGoldUpgradeData, int> UpgradeLevelChanged;

    public IReadOnlyList<CoreGoldUpgradeData> UpgradeData => _upgradeData;
    public IReadOnlyList<string> ConfigurationErrors => _configurationErrors;

    public CoreGoldUpgradeManager(
        GoldManager goldManager,
        GameStateManager gameStateManager,
        ICoreGoldUpgradeApplier upgradeApplier,
        IReadOnlyList<CoreGoldUpgradeData> upgradeData)
    {
        _goldManager = goldManager ?? throw new ArgumentNullException(nameof(goldManager));
        _gameStateManager = gameStateManager ?? throw new ArgumentNullException(nameof(gameStateManager));
        _upgradeApplier = upgradeApplier ?? throw new ArgumentNullException(nameof(upgradeApplier));

        RegisterUpgradeData(upgradeData);
    }

    public int GetLevel(CoreGoldUpgradeData data)
    {
        return data != null && _levels.TryGetValue(data, out int level) ? level : 0;
    }

    public float GetTotalFlatIncrease(CoreGoldUpgradeData data)
    {
        if (data == null || !_levels.TryGetValue(data, out int level))
            return 0f;

        return data.FlatIncreasePerLevel * level;
    }

    public CoreGoldUpgradePurchaseResult GetPurchaseResult(CoreGoldUpgradeData data, out int nextCost)
    {
        nextCost = 0;

        if (data == null || !_levels.TryGetValue(data, out int level))
            return CoreGoldUpgradePurchaseResult.InvalidData;

        if (_isPurchasing)
            return CoreGoldUpgradePurchaseResult.PurchaseInProgress;

        if (_gameStateManager.CurrentState != GameState.Playing)
            return CoreGoldUpgradePurchaseResult.NotPlaying;

        if (data.IsMaxLevelReached(level))
            return CoreGoldUpgradePurchaseResult.MaxLevel;

        if (!data.TryGetNextCost(level, out nextCost) || nextCost <= 0)
            return CoreGoldUpgradePurchaseResult.InvalidCost;

        if (!_upgradeApplier.CanApplyCoreGoldUpgrade(data.StatType, data.FlatIncreasePerLevel))
            return CoreGoldUpgradePurchaseResult.InvalidData;

        if (_goldManager.CurrentGold < nextCost)
            return CoreGoldUpgradePurchaseResult.InsufficientGold;

        return CoreGoldUpgradePurchaseResult.Success;
    }

    public CoreGoldUpgradePurchaseResult TryPurchase(CoreGoldUpgradeData data)
    {
        CoreGoldUpgradePurchaseResult validation = GetPurchaseResult(data, out int cost);

        if (validation != CoreGoldUpgradePurchaseResult.Success)
            return validation;

        _isPurchasing = true;
        bool appliedUpgrade = false;
        int goldBeforePurchase = _goldManager.CurrentGold;
        int purchasedLevel = 0;
        CoreGoldUpgradePurchaseResult result;

        try
        {
            if (!_goldManager.TrySpendGold(cost))
            {
                result = CoreGoldUpgradePurchaseResult.InsufficientGold;
            }
            else
            {
                if (!_upgradeApplier.TryApplyCoreGoldUpgrade(data.StatType, data.FlatIncreasePerLevel))
                {
                    RestoreGold(goldBeforePurchase);
                    result = CoreGoldUpgradePurchaseResult.ApplyFailed;
                }
                else
                {
                    appliedUpgrade = true;
                    purchasedLevel = _levels[data] + 1;
                    _levels[data] = purchasedLevel;
                    result = CoreGoldUpgradePurchaseResult.Success;
                }
            }
        }
        catch
        {
            if (!appliedUpgrade)
                RestoreGold(goldBeforePurchase);

            result = CoreGoldUpgradePurchaseResult.ApplyFailed;
        }
        finally
        {
            _isPurchasing = false;
        }

        if (result == CoreGoldUpgradePurchaseResult.Success)
            UpgradeLevelChanged?.Invoke(data, purchasedLevel);

        return result;
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
            // GoldManager updates its balance before publishing GoldChanged, so the refund is complete.
        }
    }

    private void RegisterUpgradeData(IReadOnlyList<CoreGoldUpgradeData> upgradeData)
    {
        if (upgradeData == null)
        {
            _configurationErrors.Add("Core gold upgrade data list is null.");
            return;
        }

        var registeredIds = new HashSet<string>(StringComparer.Ordinal);

        for (int i = 0; i < upgradeData.Count; i++)
        {
            CoreGoldUpgradeData data = upgradeData[i];

            if (data == null)
            {
                _configurationErrors.Add($"Core gold upgrade data at index {i} is null.");
                continue;
            }

            if (!data.IsConfigurationValid(out string error))
            {
                _configurationErrors.Add(error);
                continue;
            }

            if (!registeredIds.Add(data.UpgradeId))
            {
                _configurationErrors.Add($"Duplicate core gold upgrade id: '{data.UpgradeId}'.");
                continue;
            }

            _upgradeData.Add(data);
            _levels.Add(data, 0);
        }
    }
}
