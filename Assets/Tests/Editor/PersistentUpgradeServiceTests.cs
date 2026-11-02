using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public sealed class PersistentUpgradeServiceTests
{
    private PersistentUpgradeCatalog _catalog;
    private MemoryWallet _wallet;
    private MemoryStore _store;
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
        _store = new MemoryStore();
        _service = new PersistentUpgradeService(_catalog, _wallet, _store);
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

    [Test]
    public void LoadModifiers_CombinesCoreAndMatchingTowerWithExistingStatFormula()
    {
        PurchaseLevels(
            PermanentUpgradeTarget.Core,
            TowerType.Basic,
            TowerStatType.AttackDamage,
            PermanentUpgradeKind.Flat,
            10);
        PurchaseLevels(
            PermanentUpgradeTarget.Core,
            TowerType.Basic,
            TowerStatType.AttackDamage,
            PermanentUpgradeKind.Rate,
            1);
        PurchaseLevels(
            PermanentUpgradeTarget.Tower,
            TowerType.Basic,
            TowerStatType.AttackDamage,
            PermanentUpgradeKind.Flat,
            5);

        var coreModifiers = new TowerStatModifiers();
        var basicModifiers = new TowerStatModifiers();
        var rapidModifiers = new TowerStatModifiers();

        Assert.IsTrue(_service.TryLoadModifiers(
            PermanentUpgradeTarget.Core,
            TowerType.Basic,
            coreModifiers));
        Assert.IsTrue(_service.TryLoadModifiers(
            PermanentUpgradeTarget.Tower,
            TowerType.Basic,
            basicModifiers));
        Assert.IsTrue(_service.TryLoadModifiers(
            PermanentUpgradeTarget.Tower,
            TowerType.Rapid,
            rapidModifiers));

        Assert.That(
            coreModifiers.Calculate(TowerStatType.AttackDamage, 100f, basicModifiers),
            Is.EqualTo(116.15f).Within(0.0001f));
        Assert.That(
            coreModifiers.Calculate(TowerStatType.AttackDamage, 100f, rapidModifiers),
            Is.EqualTo(111.1f).Within(0.0001f));
    }

    [Test]
    public void LoadModifiers_ReplacesSnapshotAndDoesNotReadStoreDuringStatCalculation()
    {
        PurchaseLevels(
            PermanentUpgradeTarget.Core,
            TowerType.Basic,
            TowerStatType.MaxHp,
            PermanentUpgradeKind.Flat,
            2);
        var modifiers = new TowerStatModifiers();
        int readsBeforeLoad = _store.ReadCount;

        Assert.IsTrue(_service.TryLoadModifiers(
            PermanentUpgradeTarget.Core,
            TowerType.Basic,
            modifiers));
        int readsAfterLoad = _store.ReadCount;
        Assert.That(readsAfterLoad - readsBeforeLoad, Is.EqualTo(6));
        Assert.That(modifiers.GetFlat(TowerStatType.MaxHp), Is.EqualTo(4f));

        Assert.IsTrue(_service.TryLoadModifiers(
            PermanentUpgradeTarget.Core,
            TowerType.Basic,
            modifiers));
        Assert.That(modifiers.GetFlat(TowerStatType.MaxHp), Is.EqualTo(4f));

        int readsBeforeCalculation = _store.ReadCount;
        for (int i = 0; i < 100; i++)
            modifiers.Calculate(TowerStatType.MaxHp, 100f);

        Assert.That(_store.ReadCount, Is.EqualTo(readsBeforeCalculation));
    }

    [Test]
    public void ResetAllLevels_DeletesOnlyKnownUpgradeKeysAndRaisesRefreshEvent()
    {
        PurchaseLevels(
            PermanentUpgradeTarget.Core,
            TowerType.Basic,
            TowerStatType.AttackDamage,
            PermanentUpgradeKind.Flat,
            10);
        PurchaseLevels(
            PermanentUpgradeTarget.Core,
            TowerType.Basic,
            TowerStatType.AttackDamage,
            PermanentUpgradeKind.Rate,
            1);
        PurchaseLevels(
            PermanentUpgradeTarget.Tower,
            TowerType.Rapid,
            TowerStatType.MaxHp,
            PermanentUpgradeKind.Flat,
            1);
        _store.SetLevel("Unrelated.Save.Key", 37);
        int progressChangedCount = 0;
        _service.ProgressChanged += () => progressChangedCount++;

        _service.ResetAllLevels();

        foreach (PersistentUpgradeDefinition definition in _catalog.Definitions)
        {
            foreach (PermanentUpgradeKind kind in Enum.GetValues(typeof(PermanentUpgradeKind)))
            {
                Assert.That(_service.GetLevel(
                    PermanentUpgradeTarget.Core,
                    TowerType.Basic,
                    definition.StatType,
                    kind), Is.Zero);

                foreach (TowerType towerType in Enum.GetValues(typeof(TowerType)))
                {
                    Assert.That(_service.GetLevel(
                        PermanentUpgradeTarget.Tower,
                        towerType,
                        definition.StatType,
                        kind), Is.Zero);
                }
            }
        }

        Assert.That(_store.GetLevel("Unrelated.Save.Key"), Is.EqualTo(37));
        Assert.That(_store.SaveCount, Is.EqualTo(1));
        Assert.That(progressChangedCount, Is.EqualTo(1));
    }

    [Test]
    public void CurrencyWallet_AddAndReset_PreservesUnrelatedPlayerPrefsData()
    {
        string suffix = Guid.NewGuid().ToString("N");
        string currencyKey = $"Core9.Tests.Currency.{suffix}";
        string unrelatedKey = $"Core9.Tests.Unrelated.{suffix}";

        try
        {
            PlayerPrefs.SetInt(unrelatedKey, 73);
            var wallet = new PersistentCurrencyWallet(currencyKey);
            int changedCount = 0;
            int latestBalance = -1;
            wallet.BalanceChanged += balance =>
            {
                changedCount++;
                latestBalance = balance;
            };

            Assert.That(wallet.Add(5_000), Is.True);
            Assert.That(wallet.Add(5_000), Is.True);
            Assert.That(wallet.Balance, Is.EqualTo(10_000));

            wallet.Reset();

            Assert.That(wallet.Balance, Is.Zero);
            Assert.That(latestBalance, Is.Zero);
            Assert.That(changedCount, Is.EqualTo(3));
            Assert.That(PlayerPrefs.GetInt(unrelatedKey), Is.EqualTo(73));
        }
        finally
        {
            PlayerPrefs.DeleteKey(currencyKey);
            PlayerPrefs.DeleteKey(unrelatedKey);
            PlayerPrefs.Save();
        }
    }

    private void PurchaseLevels(
        PermanentUpgradeTarget target,
        TowerType towerType,
        TowerStatType statType,
        PermanentUpgradeKind kind,
        int count)
    {
        for (int i = 0; i < count; i++)
        {
            Assert.IsTrue(_service.TryPurchase(target, towerType, statType, kind));
        }
    }

    private sealed class MemoryStore : IPersistentUpgradeStore
    {
        private readonly Dictionary<string, int> _levels = new();
        public int ReadCount { get; private set; }
        public int SaveCount { get; private set; }

        public int GetLevel(string key)
        {
            ReadCount++;
            return _levels.TryGetValue(key, out int level) ? level : 0;
        }

        public void SetLevel(string key, int level) => _levels[key] = level;
        public void DeleteLevel(string key) => _levels.Remove(key);
        public void Save() => SaveCount++;
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
