using System;
using UnityEngine;

public interface IPersistentCurrencySpender
{
    bool TrySpend(int amount);
}

public interface IPersistentUpgradeStore
{
    int GetLevel(string key);
    void SetLevel(string key, int level);
}

public sealed class PlayerPrefsPersistentUpgradeStore : IPersistentUpgradeStore
{
    public int GetLevel(string key) => Mathf.Max(0, PlayerPrefs.GetInt(key, 0));

    public void SetLevel(string key, int level)
    {
        PlayerPrefs.SetInt(key, Mathf.Max(0, level));
        PlayerPrefs.Save();
    }
}

public sealed class PersistentUpgradeService
{
    private const string KeyPrefix = "Core9.PersistentUpgrade";

    private readonly PersistentUpgradeCatalog _catalog;
    private readonly IPersistentCurrencyWallet _wallet;
    private readonly IPersistentCurrencySpender _currencySpender;
    private readonly IPersistentUpgradeStore _store;

    public event Action ProgressChanged;

    public PersistentUpgradeService(
        PersistentUpgradeCatalog catalog,
        IPersistentCurrencyWallet wallet,
        IPersistentUpgradeStore store = null)
    {
        _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
        _wallet = wallet ?? throw new ArgumentNullException(nameof(wallet));
        _currencySpender = wallet as IPersistentCurrencySpender;
        _store = store ?? new PlayerPrefsPersistentUpgradeStore();
    }

    public int GetLevel(
        PermanentUpgradeTarget target,
        TowerType towerType,
        TowerStatType statType,
        PermanentUpgradeKind kind)
    {
        return _store.GetLevel(CreateKey(target, towerType, statType, kind));
    }

    public bool IsUnlocked(
        PermanentUpgradeTarget target,
        TowerType towerType,
        TowerStatType statType,
        PermanentUpgradeKind kind)
    {
        if (kind == PermanentUpgradeKind.Flat)
            return true;

        return _catalog.TryGetDefinition(statType, out PersistentUpgradeDefinition definition) &&
            GetLevel(
                target,
                towerType,
                statType,
                PermanentUpgradeKind.Flat) >= definition.RateUnlockFlatLevel;
    }

    public bool TryPurchase(
        PermanentUpgradeTarget target,
        TowerType towerType,
        TowerStatType statType,
        PermanentUpgradeKind kind)
    {
        if (_currencySpender == null ||
            !_catalog.TryGetDefinition(statType, out PersistentUpgradeDefinition definition) ||
            !IsUnlocked(target, towerType, statType, kind))
        {
            return false;
        }

        int currentLevel = GetLevel(target, towerType, statType, kind);
        int cost = definition.GetNextCost(kind, currentLevel);

        if (_wallet.Balance < cost || !_currencySpender.TrySpend(cost))
            return false;

        _store.SetLevel(CreateKey(target, towerType, statType, kind), currentLevel + 1);
        ProgressChanged?.Invoke();
        return true;
    }

    private static string CreateKey(
        PermanentUpgradeTarget target,
        TowerType towerType,
        TowerStatType statType,
        PermanentUpgradeKind kind)
    {
        string targetId = target == PermanentUpgradeTarget.Core
            ? "Core"
            : $"Tower.{towerType}";
        return $"{KeyPrefix}.{targetId}.{statType}.{kind}";
    }
}
