using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public sealed class PersistentUpgradeServiceTests
{
    private PersistentUpgradeCatalog _catalog;
    private MemoryWallet _wallet;
    private PersistentUpgradeService _service;

    [SetUp]
    public void SetUp()
    {
        _catalog = ScriptableObject.CreateInstance<PersistentUpgradeCatalog>();
        var attackDamage = new PersistentUpgradeDefinition();
        attackDamage.Configure(TowerStatType.AttackDamage, "Attack Damage", 1f, 0.01f, 10, 200, 10);
        var attackSpeed = new PersistentUpgradeDefinition();
        attackSpeed.Configure(TowerStatType.AttackSpeed, "Attack Speed", 0.05f, 0.01f, 10, 200, 10);
        var maxHp = new PersistentUpgradeDefinition();
        maxHp.Configure(TowerStatType.MaxHp, "Max HP", 2f, 0.01f, 10, 200, 10);
        _catalog.Configure(
            new List<TowerPartData>(),
            new List<PersistentUpgradeDefinition> { attackDamage, attackSpeed, maxHp });
        _wallet = new MemoryWallet(5_000);
        _service = new PersistentUpgradeService(_catalog, _wallet, new MemoryStore());
    }

    [TearDown]
    public void TearDown()
    {
        UnityEngine.Object.DestroyImmediate(_catalog);
    }

    [Test]
    public void RateUnlock_IsIndependentForEachStat()
    {
        for (int i = 0; i < 10; i++)
        {
            Assert.IsTrue(_service.TryPurchase(
                PermanentUpgradeTarget.Core,
                TowerType.Basic,
                TowerStatType.AttackDamage,
                PermanentUpgradeKind.Flat));
        }

        Assert.IsTrue(_service.IsUnlocked(
            PermanentUpgradeTarget.Core,
            TowerType.Basic,
            TowerStatType.AttackDamage,
            PermanentUpgradeKind.Rate));
        Assert.IsFalse(_service.IsUnlocked(
            PermanentUpgradeTarget.Core,
            TowerType.Basic,
            TowerStatType.AttackSpeed,
            PermanentUpgradeKind.Rate));
    }

    [Test]
    public void Purchase_SpendsPersistentCurrencyAndIncrementsLevel()
    {
        Assert.IsTrue(_service.TryPurchase(
            PermanentUpgradeTarget.Core,
            TowerType.Basic,
            TowerStatType.MaxHp,
            PermanentUpgradeKind.Flat));

        Assert.AreEqual(4_990, _wallet.Balance);
        Assert.AreEqual(1, _service.GetLevel(
            PermanentUpgradeTarget.Core,
            TowerType.Basic,
            TowerStatType.MaxHp,
            PermanentUpgradeKind.Flat));
    }

    [Test]
    public void TowerProgress_IsStoredPerTowerType()
    {
        Assert.IsTrue(_service.TryPurchase(
            PermanentUpgradeTarget.Tower,
            TowerType.Basic,
            TowerStatType.AttackDamage,
            PermanentUpgradeKind.Flat));

        Assert.AreEqual(1, _service.GetLevel(
            PermanentUpgradeTarget.Tower,
            TowerType.Basic,
            TowerStatType.AttackDamage,
            PermanentUpgradeKind.Flat));
        Assert.AreEqual(0, _service.GetLevel(
            PermanentUpgradeTarget.Tower,
            TowerType.Rapid,
            TowerStatType.AttackDamage,
            PermanentUpgradeKind.Flat));
    }

    private sealed class MemoryStore : IPersistentUpgradeStore
    {
        private readonly Dictionary<string, int> _levels = new();

        public int GetLevel(string key) => _levels.TryGetValue(key, out int level) ? level : 0;
        public void SetLevel(string key, int level) => _levels[key] = level;
    }

    private sealed class MemoryWallet : IPersistentCurrencyWallet, IPersistentCurrencySpender
    {
        public int Balance { get; private set; }
        public event Action<int> BalanceChanged;

        public MemoryWallet(int balance)
        {
            Balance = balance;
        }

        public bool Add(int amount)
        {
            if (amount <= 0)
                return false;

            Balance += amount;
            BalanceChanged?.Invoke(Balance);
            return true;
        }

        public bool TrySpend(int amount)
        {
            if (amount <= 0 || Balance < amount)
                return false;

            Balance -= amount;
            BalanceChanged?.Invoke(Balance);
            return true;
        }
    }
}
