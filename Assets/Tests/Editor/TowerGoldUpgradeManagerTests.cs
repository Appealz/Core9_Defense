using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

public sealed class TowerGoldUpgradeManagerTests
{
    private readonly List<Object> _createdObjects = new();

    [TearDown]
    public void TearDown()
    {
        for (int i = 0; i < _createdObjects.Count; i++)
        {
            if (_createdObjects[i] != null)
                Object.DestroyImmediate(_createdObjects[i]);
        }

        _createdObjects.Clear();
    }

    [Test]
    public void Purchase_StoresLevelAndModifierOnlyForTargetTower()
    {
        TowerGoldUpgradeData data = CreateData("damage", TowerStatType.AttackDamage, 2f, 10, 5f);
        Tower first = CreateTower("First");
        Tower second = CreateTower("Second");
        var applier = new FakeTowerUpgradeApplier(first, second);
        GoldManager gold = CreateGold(30);
        GameStateManager state = CreatePlayingState();
        using var manager = new TowerGoldUpgradeManager(gold, state, applier, new[] { data });

        Assert.That(manager.TryPurchase(first, data), Is.EqualTo(TowerGoldUpgradePurchaseResult.Success));
        Assert.That(manager.GetLevel(first, data), Is.EqualTo(1));
        Assert.That(manager.GetLevel(second, data), Is.Zero);
        Assert.That(manager.GetTotalFlatIncrease(first, data), Is.EqualTo(2f));
        Assert.That(applier.GetApplied(first, TowerStatType.AttackDamage), Is.EqualTo(2f));
        Assert.That(applier.GetApplied(second, TowerStatType.AttackDamage), Is.Zero);
        Assert.That(gold.CurrentGold, Is.EqualTo(20));
    }

    [Test]
    public void Purchase_WhenApplyFails_RefundsAndDoesNotAdvanceLevel()
    {
        TowerGoldUpgradeData data = CreateData("hp", TowerStatType.MaxHp, 10f, 10, 0f);
        Tower tower = CreateTower("Target");
        var applier = new FakeTowerUpgradeApplier(tower) { ApplySucceeds = false };
        GoldManager gold = CreateGold(10);
        GameStateManager state = CreatePlayingState();
        using var manager = new TowerGoldUpgradeManager(gold, state, applier, new[] { data });

        Assert.That(manager.TryPurchase(tower, data), Is.EqualTo(TowerGoldUpgradePurchaseResult.ApplyFailed));
        Assert.That(gold.CurrentGold, Is.EqualTo(10));
        Assert.That(manager.GetLevel(tower, data), Is.Zero);
    }

    [Test]
    public void Purchase_WithInsufficientGold_DoesNotSpendApplyOrAdvanceLevel()
    {
        TowerGoldUpgradeData data = CreateData("damage", TowerStatType.AttackDamage, 2f, 10, 0f);
        Tower tower = CreateTower("Target");
        var applier = new FakeTowerUpgradeApplier(tower);
        GoldManager gold = CreateGold(9);
        GameStateManager state = CreatePlayingState();
        using var manager = new TowerGoldUpgradeManager(gold, state, applier, new[] { data });

        Assert.That(manager.TryPurchase(tower, data), Is.EqualTo(TowerGoldUpgradePurchaseResult.InsufficientGold));
        Assert.That(gold.CurrentGold, Is.EqualTo(9));
        Assert.That(manager.GetLevel(tower, data), Is.Zero);
        Assert.That(applier.GetApplied(tower, TowerStatType.AttackDamage), Is.Zero);
    }

    [Test]
    public void Purchase_AtConfiguredMaxLevel_RejectsAdditionalPurchase()
    {
        TowerGoldUpgradeData data = CreateData("hp", TowerStatType.MaxHp, 10f, 10, 0f);
        SerializedObject serialized = new(data);
        serialized.FindProperty("_hasMaxLevel").boolValue = true;
        serialized.FindProperty("_maxLevel").intValue = 1;
        serialized.ApplyModifiedPropertiesWithoutUndo();
        Tower tower = CreateTower("Target");
        var applier = new FakeTowerUpgradeApplier(tower);
        GoldManager gold = CreateGold(20);
        GameStateManager state = CreatePlayingState();
        using var manager = new TowerGoldUpgradeManager(gold, state, applier, new[] { data });

        Assert.That(manager.TryPurchase(tower, data), Is.EqualTo(TowerGoldUpgradePurchaseResult.Success));
        Assert.That(manager.TryPurchase(tower, data), Is.EqualTo(TowerGoldUpgradePurchaseResult.MaxLevel));
        Assert.That(gold.CurrentGold, Is.EqualTo(10));
        Assert.That(manager.GetLevel(tower, data), Is.EqualTo(1));
    }

    [Test]
    public void TowerDestroyedBeforePurchase_RejectsWithoutSpending()
    {
        TowerGoldUpgradeData data = CreateData("speed", TowerStatType.AttackSpeed, 0.05f, 10, 0f);
        Tower tower = CreateTower("Target");
        var applier = new FakeTowerUpgradeApplier(tower);
        GoldManager gold = CreateGold(10);
        GameStateManager state = CreatePlayingState();
        using var manager = new TowerGoldUpgradeManager(gold, state, applier, new[] { data });
        applier.Remove(tower);

        Assert.That(manager.TryPurchase(tower, data), Is.EqualTo(TowerGoldUpgradePurchaseResult.TowerNotAvailable));
        Assert.That(gold.CurrentGold, Is.EqualTo(10));
        Assert.That(manager.GetLevel(tower, data), Is.Zero);
    }

    [Test]
    public void PurchaseThenTowerDestroyed_KeepsSpentGoldAndDeletesInstanceLevels()
    {
        TowerGoldUpgradeData data = CreateData("damage", TowerStatType.AttackDamage, 1f, 10, 0f);
        Tower tower = CreateTower("Target");
        var applier = new FakeTowerUpgradeApplier(tower);
        GoldManager gold = CreateGold(20);
        GameStateManager state = CreatePlayingState();
        using var manager = new TowerGoldUpgradeManager(gold, state, applier, new[] { data });

        Assert.That(manager.TryPurchase(tower, data), Is.EqualTo(TowerGoldUpgradePurchaseResult.Success));
        applier.Remove(tower);

        Assert.That(gold.CurrentGold, Is.EqualTo(10));
        Assert.That(manager.GetLevel(tower, data), Is.Zero);
    }

    [Test]
    public void TowerDestroyedDuringGoldEvent_ApplyFailsAndGoldIsRestored()
    {
        TowerGoldUpgradeData data = CreateData("damage", TowerStatType.AttackDamage, 1f, 10, 0f);
        Tower tower = CreateTower("Target");
        var applier = new FakeTowerUpgradeApplier(tower);
        GoldManager gold = CreateGold(10);
        GameStateManager state = CreatePlayingState();
        using var manager = new TowerGoldUpgradeManager(gold, state, applier, new[] { data });
        gold.GoldChanged += _ => applier.Remove(tower);

        Assert.That(manager.TryPurchase(tower, data), Is.EqualTo(TowerGoldUpgradePurchaseResult.ApplyFailed));
        Assert.That(gold.CurrentGold, Is.EqualTo(10));
        Assert.That(manager.GetLevel(tower, data), Is.Zero);
    }

    [Test]
    public void Selection_ClearsOnTowerRemovalAndWhenLeavingPlaying()
    {
        Tower first = CreateTower("First");
        Tower second = CreateTower("Second");
        var applier = new FakeTowerUpgradeApplier(first, second);
        GameStateManager state = CreatePlayingState();
        using var selection = new TowerSelectionManager(state, applier);

        Assert.That(selection.TrySelect(first), Is.True);
        Assert.That(selection.SelectedTower, Is.EqualTo(first));
        applier.Remove(first);
        Assert.That(selection.SelectedTower, Is.Null);

        Assert.That(selection.TrySelect(second), Is.True);
        state.ChangeState(GameState.WaveClear);
        Assert.That(selection.SelectedTower, Is.Null);
    }

    [Test]
    public void BattleScene_HasConfiguredTwoColumnTowerUpgradeOverlay()
    {
        EditorSceneManager.OpenScene("Assets/Scenes/Battle.unity", OpenSceneMode.Single);
        GameObject panel = FindInactiveGameObject("TowerGoldUpgradePanel");
        Assert.That(panel, Is.Not.Null);
        Assert.That(panel.activeSelf, Is.False);

        TowerGoldUpgradeView view = panel.GetComponent<TowerGoldUpgradeView>();
        ScrollRect scrollRect = panel.GetComponent<ScrollRect>();
        Transform content = panel.transform.Find("Viewport/Content");
        Assert.That(view, Is.Not.Null);
        Assert.That(view.IsConfigured, Is.True);
        Assert.That(scrollRect.horizontal, Is.False);
        Assert.That(scrollRect.vertical, Is.True);
        Assert.That(content, Is.Not.Null);
        Assert.That(panel.transform.Find("Header/CloseButton").GetComponent<Button>(), Is.Not.Null);

        GridLayoutGroup grid = content.GetComponent<GridLayoutGroup>();
        ContentSizeFitter fitter = content.GetComponent<ContentSizeFitter>();
        Assert.That(grid.constraint, Is.EqualTo(GridLayoutGroup.Constraint.FixedColumnCount));
        Assert.That(grid.constraintCount, Is.EqualTo(2));
        Assert.That(fitter.verticalFit, Is.EqualTo(ContentSizeFitter.FitMode.PreferredSize));

        GameObject corePanel = GameObject.Find("CoreGoldUpgradePanel");
        Assert.That(panel.transform.parent, Is.EqualTo(corePanel.transform.parent));
        Assert.That(panel.transform.GetSiblingIndex(), Is.GreaterThan(corePanel.transform.GetSiblingIndex()));

        RectTransform panelRect = panel.GetComponent<RectTransform>();
        RectTransform coreRect = corePanel.GetComponent<RectTransform>();
        Assert.That(panelRect.anchorMin, Is.EqualTo(coreRect.anchorMin));
        Assert.That(panelRect.anchorMax, Is.EqualTo(coreRect.anchorMax));

        GameStartData startData = AssetDatabase.LoadAssetAtPath<GameStartData>(
            "Assets/Script/Game/GameStartData.asset");
        Assert.That(startData.TowerGoldUpgradeDataList, Has.Count.EqualTo(3));

        for (int i = 0; i < startData.TowerGoldUpgradeDataList.Count; i++)
            Assert.That(startData.TowerGoldUpgradeDataList[i].IsConfigurationValid(out _), Is.True);
    }

    [Test]
    public void ModifierCalculation_IncludesIndividualTowerSource()
    {
        var global = new TowerStatModifiers();
        var core = new TowerStatModifiers();
        var slot = new TowerStatModifiers();
        var type = new TowerStatModifiers();
        var individual = new TowerStatModifiers();
        global.TryAdd(TowerStatType.AttackDamage, StatModifierType.Flat, 1f);
        core.TryAdd(TowerStatType.AttackDamage, StatModifierType.Flat, 2f);
        slot.TryAdd(TowerStatType.AttackDamage, StatModifierType.Flat, 3f);
        type.TryAdd(TowerStatType.AttackDamage, StatModifierType.Flat, 4f);
        individual.TryAdd(TowerStatType.AttackDamage, StatModifierType.Flat, 5f);

        float result = global.Calculate(
            TowerStatType.AttackDamage,
            10f,
            core,
            slot,
            type,
            individual);

        Assert.That(result, Is.EqualTo(25f));
    }

    [UnityTest]
    public IEnumerator BattleScene_CardClickPurchasesSelectedTowerUpgradeAndCloseClearsOverlay()
    {
        EditorSceneManager.OpenScene("Assets/Scenes/Battle.unity", OpenSceneMode.Single);
        yield return new EnterPlayMode();

        GameBootstrapper bootstrapper = null;
        Tower tower = null;

        float startupDeadline = Time.realtimeSinceStartup + 15f;

        while ((bootstrapper == null || tower == null) &&
               Time.realtimeSinceStartup < startupDeadline)
        {
            bootstrapper = Object.FindAnyObjectByType<GameBootstrapper>();
            tower = Object.FindAnyObjectByType<Tower>();
            yield return null;
        }

        Assert.That(bootstrapper, Is.Not.Null, "Battle bootstrapper did not start in Play Mode.");
        Assert.That(tower, Is.Not.Null, "Initial tower was not created in Play Mode.");

        GoldManager gold = GetPrivateField<GoldManager>(bootstrapper, "_goldManager");
        TowerSelectionManager selection = GetPrivateField<TowerSelectionManager>(
            bootstrapper,
            "_towerSelectionManager");
        TowerGoldUpgradeManager upgrades = GetPrivateField<TowerGoldUpgradeManager>(
            bootstrapper,
            "_towerGoldUpgradeManager");
        TowerGoldUpgradeView view = GetPrivateField<TowerGoldUpgradeView>(
            bootstrapper,
            "_towerGoldUpgradeView");

        Assert.That(gold, Is.Not.Null);
        Assert.That(selection, Is.Not.Null);
        Assert.That(upgrades, Is.Not.Null);
        Assert.That(view, Is.Not.Null);

        gold.AddGold(500);
        int goldBefore = gold.CurrentGold;
        float damageBefore = tower.Stats.AttackDamage;
        float speedBefore = tower.Stats.AttackSpeed;
        float maxHpBefore = tower.Stats.MaxHp;
        TowerGoldUpgradeData damageData = upgrades.UpgradeData[0];
        TowerGoldUpgradeData speedData = upgrades.UpgradeData[1];
        TowerGoldUpgradeData maxHpData = upgrades.UpgradeData[2];
        Assert.That(damageData.StatType, Is.EqualTo(TowerStatType.AttackDamage));
        Assert.That(speedData.StatType, Is.EqualTo(TowerStatType.AttackSpeed));
        Assert.That(maxHpData.StatType, Is.EqualTo(TowerStatType.MaxHp));
        Assert.That(damageData.TryGetNextCost(0, out int damageCost), Is.True);
        Assert.That(speedData.TryGetNextCost(0, out int speedCost), Is.True);
        Assert.That(maxHpData.TryGetNextCost(0, out int maxHpCost), Is.True);
        Assert.That(selection.TrySelect(tower), Is.True);
        yield return null;

        GameObject panel = view.gameObject;
        Assert.That(panel.activeSelf, Is.True);
        LineRenderer selectionLine = tower.transform
            .Find("GoldUpgradeSelection")
            .GetComponent<LineRenderer>();
        Assert.That(selectionLine.enabled, Is.True);
        Transform content = panel.transform.Find("Viewport/Content");
        Assert.That(content.childCount, Is.EqualTo(3));
        Button damageCard = content.GetChild(0).GetComponent<Button>();
        Button speedCard = content.GetChild(1).GetComponent<Button>();
        Button maxHpCard = content.GetChild(2).GetComponent<Button>();
        Assert.That(damageCard, Is.Not.Null);
        Assert.That(speedCard, Is.Not.Null);
        Assert.That(maxHpCard, Is.Not.Null);
        damageCard.onClick.Invoke();
        speedCard.onClick.Invoke();
        maxHpCard.onClick.Invoke();

        Assert.That(upgrades.GetLevel(tower, damageData), Is.EqualTo(1));
        Assert.That(upgrades.GetLevel(tower, speedData), Is.EqualTo(1));
        Assert.That(upgrades.GetLevel(tower, maxHpData), Is.EqualTo(1));
        Assert.That(gold.CurrentGold, Is.EqualTo(goldBefore - damageCost - speedCost - maxHpCost));
        Assert.That(tower.Stats.AttackDamage,
            Is.EqualTo(damageBefore + damageData.FlatIncreasePerLevel).Within(0.0001f));
        Assert.That(tower.Stats.AttackSpeed,
            Is.EqualTo(speedBefore + speedData.FlatIncreasePerLevel).Within(0.0001f));
        Assert.That(tower.Stats.MaxHp,
            Is.EqualTo(maxHpBefore + maxHpData.FlatIncreasePerLevel).Within(0.0001f));
        Assert.That(tower.Health.MaxHp, Is.EqualTo(tower.Stats.MaxHp).Within(0.0001f));

        Button closeButton = panel.transform.Find("Header/CloseButton").GetComponent<Button>();
        closeButton.onClick.Invoke();
        yield return null;
        Assert.That(selection.SelectedTower, Is.Null);
        Assert.That(panel.activeSelf, Is.False);

        yield return new ExitPlayMode();
    }

    private Tower CreateTower(string displayName)
    {
        GameObject towerObject = new(displayName);
        _createdObjects.Add(towerObject);
        towerObject.AddComponent<SpriteRenderer>();
        towerObject.AddComponent<TowerHitFlash>();
        Tower tower = towerObject.AddComponent<Tower>();
        typeof(Tower).GetMethod("Awake", BindingFlags.Instance | BindingFlags.NonPublic)
            ?.Invoke(tower, null);
        tower.Initialize(
            new TowerHealth(100f),
            null,
            null,
            null,
            TowerType.Basic,
            null,
            displayName);
        return tower;
    }

    private TowerGoldUpgradeData CreateData(
        string id,
        TowerStatType statType,
        float flatIncrease,
        int baseCost,
        float growth)
    {
        TowerGoldUpgradeData data = ScriptableObject.CreateInstance<TowerGoldUpgradeData>();
        _createdObjects.Add(data);
        SerializedObject serialized = new(data);
        serialized.FindProperty("_upgradeId").stringValue = id;
        serialized.FindProperty("_displayName").stringValue = id;
        serialized.FindProperty("_statType").enumValueIndex = (int)statType;
        serialized.FindProperty("_flatIncreasePerLevel").floatValue = flatIncrease;
        serialized.FindProperty("_baseCost").intValue = baseCost;
        serialized.FindProperty("_costGrowth").enumValueIndex = (int)CoreGoldUpgradeCostGrowth.Linear;
        serialized.FindProperty("_costGrowthValue").floatValue = growth;
        serialized.ApplyModifiedPropertiesWithoutUndo();
        return data;
    }

    private static GoldManager CreateGold(int amount)
    {
        var manager = new GoldManager();
        manager.AddGold(amount);
        return manager;
    }

    private static GameStateManager CreatePlayingState()
    {
        var manager = new GameStateManager();
        manager.ChangeState(GameState.Playing);
        return manager;
    }

    private static GameObject FindInactiveGameObject(string name)
    {
        GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();

        for (int i = 0; i < allObjects.Length; i++)
        {
            if (allObjects[i].name == name && allObjects[i].scene.IsValid())
                return allObjects[i];
        }

        return null;
    }

    private static T GetPrivateField<T>(object target, string fieldName) where T : class
    {
        return target.GetType()
            .GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic)
            ?.GetValue(target) as T;
    }

    private sealed class FakeTowerUpgradeApplier : ITowerGoldUpgradeApplier
    {
        private readonly HashSet<Tower> _active = new();
        private readonly Dictionary<(Tower tower, TowerStatType stat), float> _applied = new();

        public event Action<Tower> TowerRemoved;
        public bool ApplySucceeds { get; set; } = true;

        public FakeTowerUpgradeApplier(params Tower[] towers)
        {
            for (int i = 0; i < towers.Length; i++)
                _active.Add(towers[i]);
        }

        public bool IsTowerActive(Tower tower) => tower != null && tower.IsAlive && _active.Contains(tower);

        public bool CanApplyTowerGoldUpgrade(Tower tower, TowerStatType statType, float flatValue) =>
            IsTowerActive(tower) && flatValue > 0f;

        public bool TryApplyTowerGoldUpgrade(Tower tower, TowerStatType statType, float flatValue)
        {
            if (!ApplySucceeds || !CanApplyTowerGoldUpgrade(tower, statType, flatValue))
                return false;

            (Tower, TowerStatType) key = (tower, statType);
            _applied[key] = GetApplied(tower, statType) + flatValue;
            return true;
        }

        public float GetApplied(Tower tower, TowerStatType statType) =>
            _applied.TryGetValue((tower, statType), out float value) ? value : 0f;

        public void Remove(Tower tower)
        {
            if (!_active.Remove(tower))
                return;

            TowerRemoved?.Invoke(tower);
        }
    }
}
