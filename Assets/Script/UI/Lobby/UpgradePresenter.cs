using System;

public sealed class UpgradePresenter : IDisposable
{
    private static readonly TowerStatType[] DisplayedStats =
    {
        TowerStatType.AttackDamage,
        TowerStatType.AttackSpeed,
        TowerStatType.MaxHp
    };

    private readonly UpgradeView _view;
    private readonly PersistentUpgradeCatalog _catalog;
    private readonly PersistentUpgradeService _service;
    private readonly IPersistentCurrencyWallet _wallet;

    private PermanentUpgradeTarget _selectedTarget = PermanentUpgradeTarget.Core;
    private TowerType _selectedTower = TowerType.Basic;

    public event Action BackRequested;

    public UpgradePresenter(
        UpgradeView view,
        PersistentUpgradeCatalog catalog,
        PersistentUpgradeService service,
        IPersistentCurrencyWallet wallet)
    {
        _view = view ?? throw new ArgumentNullException(nameof(view));
        _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
        _service = service ?? throw new ArgumentNullException(nameof(service));
        _wallet = wallet ?? throw new ArgumentNullException(nameof(wallet));

        _view.Initialize();
        _view.BackClicked += HandleBackClicked;
        _view.TargetClicked += HandleTargetClicked;
        _view.TowerClicked += HandleTowerClicked;
        _view.PurchaseClicked += HandlePurchaseClicked;
        _wallet.BalanceChanged += HandleBalanceChanged;
        _service.ProgressChanged += Refresh;
    }

    public void Show()
    {
        _view.SetVisible(true);
        Refresh();
    }

    public void Hide() => _view.SetVisible(false);

    private void HandleBackClicked() => BackRequested?.Invoke();

    private void HandleTargetClicked(PermanentUpgradeTarget target)
    {
        _selectedTarget = target;
        Refresh();
    }

    private void HandleTowerClicked(TowerType towerType)
    {
        _selectedTower = towerType;
        Refresh();
    }

    private void HandlePurchaseClicked(PermanentUpgradeKind kind, TowerStatType statType)
    {
        _service.TryPurchase(_selectedTarget, _selectedTower, statType, kind);
        Refresh();
    }

    private void HandleBalanceChanged(int _) => Refresh();

    private void Refresh()
    {
        var flatRows = new UpgradeRowDisplayData[DisplayedStats.Length];
        var rateRows = new UpgradeRowDisplayData[DisplayedStats.Length];

        for (int i = 0; i < DisplayedStats.Length; i++)
        {
            TowerStatType statType = DisplayedStats[i];

            if (!_catalog.TryGetDefinition(statType, out PersistentUpgradeDefinition definition))
                continue;

            flatRows[i] = CreateRow(definition, PermanentUpgradeKind.Flat);
            rateRows[i] = CreateRow(definition, PermanentUpgradeKind.Rate);
        }

        _view.Render(_selectedTarget, _selectedTower, _wallet.Balance, flatRows, rateRows);
    }

    private UpgradeRowDisplayData CreateRow(
        PersistentUpgradeDefinition definition,
        PermanentUpgradeKind kind)
    {
        int level = _service.GetLevel(
            _selectedTarget,
            _selectedTower,
            definition.StatType,
            kind);
        int nextCost = definition.GetNextCost(kind, level);
        bool isUnlocked = _service.IsUnlocked(
            _selectedTarget,
            _selectedTower,
            definition.StatType,
            kind);
        float amount = definition.GetAccumulatedAmount(kind, level);
        string amountText = FormatAmount(definition.StatType, kind, amount);

        return new UpgradeRowDisplayData(
            definition.StatType,
            kind,
            definition.DisplayName,
            level,
            amountText,
            nextCost,
            isUnlocked,
            isUnlocked && _wallet.Balance >= nextCost,
            definition.RateUnlockFlatLevel);
    }

    private static string FormatAmount(
        TowerStatType statType,
        PermanentUpgradeKind kind,
        float amount)
    {
        if (kind == PermanentUpgradeKind.Rate)
            return $"+{amount * 100f:0.##}%";

        return statType == TowerStatType.AttackSpeed
            ? $"+{amount:0.##}/s"
            : $"+{amount:0.##}";
    }

    public void Dispose()
    {
        _view.BackClicked -= HandleBackClicked;
        _view.TargetClicked -= HandleTargetClicked;
        _view.TowerClicked -= HandleTowerClicked;
        _view.PurchaseClicked -= HandlePurchaseClicked;
        _wallet.BalanceChanged -= HandleBalanceChanged;
        _service.ProgressChanged -= Refresh;
    }
}
