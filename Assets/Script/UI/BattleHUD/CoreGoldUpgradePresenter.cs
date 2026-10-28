using System;
using System.Collections.Generic;

public sealed class CoreGoldUpgradePresenter : IDisposable
{
    private readonly CoreGoldUpgradeView _view;
    private readonly CoreGoldUpgradeManager _upgradeManager;
    private readonly GoldManager _goldManager;
    private readonly GameStateManager _gameStateManager;

    public CoreGoldUpgradePresenter(
        CoreGoldUpgradeView view,
        CoreGoldUpgradeManager upgradeManager,
        GoldManager goldManager,
        GameStateManager gameStateManager)
    {
        _view = view ?? throw new ArgumentNullException(nameof(view));
        _upgradeManager = upgradeManager ?? throw new ArgumentNullException(nameof(upgradeManager));
        _goldManager = goldManager ?? throw new ArgumentNullException(nameof(goldManager));
        _gameStateManager = gameStateManager ?? throw new ArgumentNullException(nameof(gameStateManager));

        _goldManager.GoldChanged += HandleGoldChanged;
        _upgradeManager.UpgradeLevelChanged += HandleUpgradeLevelChanged;
        _gameStateManager.StateChanged += HandleStateChanged;

        _view.Build(_upgradeManager.UpgradeData, HandlePurchaseRequested);
        RefreshVisibility();
        RefreshAll();
    }

    public void Dispose()
    {
        _goldManager.GoldChanged -= HandleGoldChanged;
        _upgradeManager.UpgradeLevelChanged -= HandleUpgradeLevelChanged;
        _gameStateManager.StateChanged -= HandleStateChanged;
        _view.Clear();
    }

    private void HandlePurchaseRequested(CoreGoldUpgradeData data)
    {
        _upgradeManager.TryPurchase(data);
    }

    private void HandleGoldChanged(int _)
    {
        RefreshAll();
    }

    private void HandleUpgradeLevelChanged(CoreGoldUpgradeData _, int __)
    {
        RefreshAll();
    }

    private void HandleStateChanged(GameState _, GameState __)
    {
        RefreshVisibility();
        RefreshAll();
    }

    private void RefreshVisibility()
    {
        _view.SetVisible(_gameStateManager.CurrentState == GameState.Playing);
    }

    private void RefreshAll()
    {
        IReadOnlyList<CoreGoldUpgradeData> upgradeData = _upgradeManager.UpgradeData;

        for (int i = 0; i < upgradeData.Count; i++)
        {
            CoreGoldUpgradeData data = upgradeData[i];
            int level = _upgradeManager.GetLevel(data);
            bool isMaxLevel = data.IsMaxLevelReached(level);
            data.TryGetNextCost(level, out int nextCost);
            CoreGoldUpgradePurchaseResult result =
                _upgradeManager.GetPurchaseResult(data, out int validatedCost);

            if (validatedCost > 0)
                nextCost = validatedCost;

            _view.SetItemState(
                data,
                level,
                FormatTotalBonus(data.StatType, _upgradeManager.GetTotalFlatIncrease(data)),
                nextCost,
                isMaxLevel,
                result == CoreGoldUpgradePurchaseResult.Success);
        }
    }

    private static string FormatTotalBonus(TowerStatType statType, float value)
    {
        return statType switch
        {
            TowerStatType.CriticalChance => $"+{value * 100f:0.##}%p",
            TowerStatType.AttackSpeed => $"+{value:0.##}/s",
            TowerStatType.CriticalDamage => $"+{value:0.##}x",
            _ => $"+{value:0.##}"
        };
    }
}
