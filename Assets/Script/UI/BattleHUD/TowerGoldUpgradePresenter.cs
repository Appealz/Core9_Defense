using System;
using System.Collections.Generic;

public sealed class TowerGoldUpgradePresenter : IDisposable
{
    private readonly TowerGoldUpgradeView _view;
    private readonly TowerGoldUpgradeManager _upgradeManager;
    private readonly TowerSelectionManager _selectionManager;
    private readonly GoldManager _goldManager;
    private readonly GameStateManager _gameStateManager;

    public TowerGoldUpgradePresenter(
        TowerGoldUpgradeView view,
        TowerGoldUpgradeManager upgradeManager,
        TowerSelectionManager selectionManager,
        GoldManager goldManager,
        GameStateManager gameStateManager)
    {
        _view = view ?? throw new ArgumentNullException(nameof(view));
        _upgradeManager = upgradeManager ?? throw new ArgumentNullException(nameof(upgradeManager));
        _selectionManager = selectionManager ?? throw new ArgumentNullException(nameof(selectionManager));
        _goldManager = goldManager ?? throw new ArgumentNullException(nameof(goldManager));
        _gameStateManager = gameStateManager ?? throw new ArgumentNullException(nameof(gameStateManager));

        _selectionManager.SelectionChanged += HandleSelectionChanged;
        _upgradeManager.UpgradeLevelChanged += HandleUpgradeLevelChanged;
        _upgradeManager.TowerUpgradeStateRemoved += HandleTowerUpgradeStateRemoved;
        _goldManager.GoldChanged += HandleGoldChanged;
        _gameStateManager.StateChanged += HandleStateChanged;

        _view.Build(_upgradeManager.UpgradeData, HandlePurchaseRequested, HandleCloseRequested);
        Refresh();
    }

    public void Dispose()
    {
        _selectionManager.SelectionChanged -= HandleSelectionChanged;
        _upgradeManager.UpgradeLevelChanged -= HandleUpgradeLevelChanged;
        _upgradeManager.TowerUpgradeStateRemoved -= HandleTowerUpgradeStateRemoved;
        _goldManager.GoldChanged -= HandleGoldChanged;
        _gameStateManager.StateChanged -= HandleStateChanged;
        _view.Clear();
        _view.SetVisible(false);
    }

    private void HandlePurchaseRequested(TowerGoldUpgradeData data)
    {
        _upgradeManager.TryPurchase(_selectionManager.SelectedTower, data);
    }

    private void HandleCloseRequested()
    {
        _selectionManager.ClearSelection();
    }

    private void HandleSelectionChanged(Tower _) => Refresh();
    private void HandleGoldChanged(int _) => RefreshItems();
    private void HandleStateChanged(GameState _, GameState __) => Refresh();
    private void HandleUpgradeLevelChanged(Tower _, TowerGoldUpgradeData __, int ___) => RefreshItems();
    private void HandleTowerUpgradeStateRemoved(Tower _) => Refresh();

    private void Refresh()
    {
        Tower tower = _selectionManager.SelectedTower;
        bool isVisible = _gameStateManager.CurrentState == GameState.Playing &&
            tower != null && tower.IsAlive;

        if (!isVisible)
        {
            _view.SetVisible(false);
            return;
        }

        _view.ShowTower(tower.DisplayName);
        RefreshItems();
    }

    private void RefreshItems()
    {
        Tower tower = _selectionManager.SelectedTower;

        if (tower == null)
            return;

        IReadOnlyList<TowerGoldUpgradeData> upgradeData = _upgradeManager.UpgradeData;

        for (int i = 0; i < upgradeData.Count; i++)
        {
            TowerGoldUpgradeData data = upgradeData[i];
            int level = _upgradeManager.GetLevel(tower, data);
            bool isMaxLevel = data.IsMaxLevelReached(level);
            data.TryGetNextCost(level, out int nextCost);
            TowerGoldUpgradePurchaseResult result =
                _upgradeManager.GetPurchaseResult(tower, data, out int validatedCost);

            if (validatedCost > 0)
                nextCost = validatedCost;

            _view.SetItemState(
                data,
                level,
                FormatTotalBonus(data.StatType, _upgradeManager.GetTotalFlatIncrease(tower, data)),
                nextCost,
                isMaxLevel,
                result == TowerGoldUpgradePurchaseResult.Success);
        }
    }

    private static string FormatTotalBonus(TowerStatType statType, float value)
    {
        return statType == TowerStatType.AttackSpeed
            ? $"+{value:0.##}/s"
            : $"+{value:0.##}";
    }
}
