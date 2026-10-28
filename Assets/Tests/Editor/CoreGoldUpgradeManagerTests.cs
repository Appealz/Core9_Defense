using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Object = UnityEngine.Object;

public sealed class CoreGoldUpgradeManagerTests
{
    private readonly List<Object> _createdObjects = new();

    [TearDown]
    public void TearDown()
    {
        for (int i = 0; i < _createdObjects.Count; i++)
            Object.DestroyImmediate(_createdObjects[i]);

        _createdObjects.Clear();
    }

    [Test]
    public void Purchase_ConsumesCurrentLevelCostAndAppliesOneFlatLevel()
    {
        CoreGoldUpgradeData data = CreateUpgradeData(
            "attack_damage",
            TowerStatType.AttackDamage,
            2f,
            10,
            CoreGoldUpgradeCostGrowth.Linear,
            5f);
        var goldManager = new GoldManager();
        goldManager.AddGold(30);
        var gameStateManager = new GameStateManager();
        gameStateManager.ChangeState(GameState.Playing);
        var applier = new FakeUpgradeApplier();
        var manager = new CoreGoldUpgradeManager(
            goldManager,
            gameStateManager,
            applier,
            new[] { data });

        Assert.That(manager.TryPurchase(data), Is.EqualTo(CoreGoldUpgradePurchaseResult.Success));
        Assert.That(manager.TryPurchase(data), Is.EqualTo(CoreGoldUpgradePurchaseResult.Success));
        Assert.That(goldManager.CurrentGold, Is.EqualTo(5));
        Assert.That(manager.GetLevel(data), Is.EqualTo(2));
        Assert.That(manager.GetTotalFlatIncrease(data), Is.EqualTo(4f));
        Assert.That(applier.ApplyCount, Is.EqualTo(2));
    }

    [Test]
    public void Purchase_OutsidePlaying_DoesNotSpendOrApply()
    {
        CoreGoldUpgradeData data = CreateUpgradeData(
            "max_hp",
            TowerStatType.MaxHp,
            10f,
            10,
            CoreGoldUpgradeCostGrowth.Linear,
            0f);
        var goldManager = new GoldManager();
        goldManager.AddGold(10);
        var gameStateManager = new GameStateManager();
        var applier = new FakeUpgradeApplier();
        var manager = new CoreGoldUpgradeManager(
            goldManager,
            gameStateManager,
            applier,
            new[] { data });

        Assert.That(manager.TryPurchase(data), Is.EqualTo(CoreGoldUpgradePurchaseResult.NotPlaying));
        Assert.That(goldManager.CurrentGold, Is.EqualTo(10));
        Assert.That(manager.GetLevel(data), Is.Zero);
        Assert.That(applier.ApplyCount, Is.Zero);
    }

    [Test]
    public void Purchase_WhenApplyFails_RefundsGoldAndKeepsLevel()
    {
        CoreGoldUpgradeData data = CreateUpgradeData(
            "critical_chance",
            TowerStatType.CriticalChance,
            0.01f,
            10,
            CoreGoldUpgradeCostGrowth.Linear,
            0f);
        var goldManager = new GoldManager();
        goldManager.AddGold(10);
        var gameStateManager = new GameStateManager();
        gameStateManager.ChangeState(GameState.Playing);
        var applier = new FakeUpgradeApplier { ApplySucceeds = false };
        var manager = new CoreGoldUpgradeManager(
            goldManager,
            gameStateManager,
            applier,
            new[] { data });

        Assert.That(manager.TryPurchase(data), Is.EqualTo(CoreGoldUpgradePurchaseResult.ApplyFailed));
        Assert.That(goldManager.CurrentGold, Is.EqualTo(10));
        Assert.That(manager.GetLevel(data), Is.Zero);
    }

    [Test]
    public void Purchase_WhenGoldChangedSubscriberThrows_RestoresGold()
    {
        CoreGoldUpgradeData data = CreateUpgradeData(
            "max_hp",
            TowerStatType.MaxHp,
            10f,
            10,
            CoreGoldUpgradeCostGrowth.Linear,
            0f);
        var goldManager = new GoldManager();
        goldManager.AddGold(10);
        var gameStateManager = new GameStateManager();
        gameStateManager.ChangeState(GameState.Playing);
        var applier = new FakeUpgradeApplier();
        var manager = new CoreGoldUpgradeManager(
            goldManager,
            gameStateManager,
            applier,
            new[] { data });
        goldManager.GoldChanged += _ => throw new InvalidOperationException("Test subscriber failure.");

        Assert.That(manager.TryPurchase(data), Is.EqualTo(CoreGoldUpgradePurchaseResult.ApplyFailed));
        Assert.That(goldManager.CurrentGold, Is.EqualTo(10));
        Assert.That(manager.GetLevel(data), Is.Zero);
        Assert.That(applier.ApplyCount, Is.Zero);
    }

    [Test]
    public void Purchase_ReentrantRequestDuringGoldEvent_IsRejected()
    {
        CoreGoldUpgradeData data = CreateUpgradeData(
            "attack_speed",
            TowerStatType.AttackSpeed,
            0.1f,
            10,
            CoreGoldUpgradeCostGrowth.Linear,
            0f);
        var goldManager = new GoldManager();
        goldManager.AddGold(20);
        var gameStateManager = new GameStateManager();
        gameStateManager.ChangeState(GameState.Playing);
        var applier = new FakeUpgradeApplier();
        var manager = new CoreGoldUpgradeManager(
            goldManager,
            gameStateManager,
            applier,
            new[] { data });
        CoreGoldUpgradePurchaseResult nestedResult = CoreGoldUpgradePurchaseResult.Success;
        goldManager.GoldChanged += _ => nestedResult = manager.TryPurchase(data);

        Assert.That(manager.TryPurchase(data), Is.EqualTo(CoreGoldUpgradePurchaseResult.Success));
        Assert.That(nestedResult, Is.EqualTo(CoreGoldUpgradePurchaseResult.PurchaseInProgress));
        Assert.That(goldManager.CurrentGold, Is.EqualTo(10));
        Assert.That(manager.GetLevel(data), Is.EqualTo(1));
    }

    [Test]
    public void Cost_ExponentialOverflow_SaturatesAtIntMaxValue()
    {
        CoreGoldUpgradeData data = CreateUpgradeData(
            "critical_damage",
            TowerStatType.CriticalDamage,
            0.1f,
            int.MaxValue,
            CoreGoldUpgradeCostGrowth.Exponential,
            2f);

        Assert.That(data.TryGetNextCost(100, out int cost), Is.True);
        Assert.That(cost, Is.EqualTo(int.MaxValue));
    }

    [Test]
    public void BattleScene_HasConfiguredUpgradePanelAndFiveValidEntries()
    {
        EditorSceneManager.OpenScene("Assets/Scenes/Battle.unity", OpenSceneMode.Single);
        GameObject bootstrapObject = GameObject.Find("GameBootstrapper");
        GameObject panelObject = GameObject.Find("CoreGoldUpgradePanel");

        Assert.That(bootstrapObject, Is.Not.Null);
        Assert.That(panelObject, Is.Not.Null);

        CoreGoldUpgradeView view = panelObject.GetComponent<CoreGoldUpgradeView>();
        Assert.That(view, Is.Not.Null);
        Assert.That(view.IsConfigured, Is.True);

        RectTransform panelRect = panelObject.GetComponent<RectTransform>();
        ScrollRect scrollRect = panelObject.GetComponent<ScrollRect>();
        Transform content = panelObject.transform.Find("Viewport/Content");
        Assert.That(panelRect.anchorMin, Is.EqualTo(new Vector2(0f, 0f)));
        Assert.That(panelRect.anchorMax, Is.EqualTo(new Vector2(1f, 0.25f)));
        Assert.That(panelRect.sizeDelta, Is.EqualTo(Vector2.zero));
        Assert.That(scrollRect.horizontal, Is.False);
        Assert.That(scrollRect.vertical, Is.True);
        Assert.That(content, Is.Not.Null);
        Assert.That(content.GetComponent<HorizontalLayoutGroup>(), Is.Null);

        GridLayoutGroup grid = content.GetComponent<GridLayoutGroup>();
        ContentSizeFitter sizeFitter = content.GetComponent<ContentSizeFitter>();
        Assert.That(grid, Is.Not.Null);
        Assert.That(grid.constraint, Is.EqualTo(GridLayoutGroup.Constraint.FixedColumnCount));
        Assert.That(grid.constraintCount, Is.EqualTo(2));
        Assert.That(sizeFitter.verticalFit, Is.EqualTo(ContentSizeFitter.FitMode.PreferredSize));

        GameStartData gameStartData = AssetDatabase.LoadAssetAtPath<GameStartData>(
            "Assets/Script/Game/GameStartData.asset");
        Assert.That(gameStartData, Is.Not.Null);
        Assert.That(gameStartData.CoreGoldUpgradeDataList, Has.Count.EqualTo(5));

        for (int i = 0; i < gameStartData.CoreGoldUpgradeDataList.Count; i++)
            Assert.That(gameStartData.CoreGoldUpgradeDataList[i].IsConfigurationValid(out _), Is.True);
    }

    [Test]
    public void UpgradeItem_UsesWholeCardButtonAndHonorsDisabledState()
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(
            "Assets/Script/UI/BattleHUD/CoreGoldUpgradeItem.prefab");
        Assert.That(prefab, Is.Not.Null);
        Assert.That(prefab.transform.Find("PurchaseButton"), Is.Null);

        GameObject instance = Object.Instantiate(prefab);
        _createdObjects.Add(instance);
        CoreGoldUpgradeItemView itemView = instance.GetComponent<CoreGoldUpgradeItemView>();
        Button cardButton = instance.GetComponent<Button>();
        Image cardBackground = instance.GetComponent<Image>();
        Assert.That(itemView, Is.Not.Null);
        Assert.That(cardButton, Is.Not.Null);

        var serializedView = new SerializedObject(itemView);
        Assert.That(serializedView.FindProperty("_cardButton").objectReferenceValue, Is.EqualTo(cardButton));
        Assert.That(serializedView.FindProperty("_cardBackground").objectReferenceValue, Is.EqualTo(cardBackground));

        CoreGoldUpgradeData data = CreateUpgradeData(
            "attack_damage",
            TowerStatType.AttackDamage,
            2f,
            50,
            CoreGoldUpgradeCostGrowth.Linear,
            5f);
        int purchaseRequestCount = 0;
        itemView.Initialize(data, _ => purchaseRequestCount++);
        itemView.SetState(0, "+0", 50, false, true);

        var pointer = new PointerEventData(null) { button = PointerEventData.InputButton.Left };
        cardButton.OnPointerClick(pointer);
        Assert.That(purchaseRequestCount, Is.EqualTo(1));

        itemView.SetState(0, "+0", 50, false, false);
        cardButton.OnPointerClick(pointer);
        Assert.That(purchaseRequestCount, Is.EqualTo(1));

        Color unavailableColor = cardBackground.color;
        itemView.SetState(10, "+20", 0, true, false);
        Assert.That(cardBackground.color, Is.Not.EqualTo(unavailableColor));
    }

    [Test]
    public void TowerStats_CriticalDamageFlatModifier_ChangesActualCriticalDamage()
    {
        TowerPartData partData = ScriptableObject.CreateInstance<TowerPartData>();
        _createdObjects.Add(partData);
        var serializedData = new SerializedObject(partData);
        serializedData.FindProperty("_maxHp").intValue = 100;
        serializedData.FindProperty("_attackDamage").floatValue = 10f;
        serializedData.FindProperty("_attackRate").floatValue = 1f;
        serializedData.FindProperty("_criticalChance").floatValue = 1f;
        serializedData.FindProperty("_criticalChanceCap").floatValue = 1f;
        serializedData.FindProperty("_criticalDamageMultiplier").floatValue = 2f;
        serializedData.FindProperty("_minimumAttackInterval").floatValue = 0.05f;
        serializedData.ApplyModifiedPropertiesWithoutUndo();

        var globalModifiers = new TowerStatModifiers();
        var coreGoldModifiers = new TowerStatModifiers();
        Assert.That(coreGoldModifiers.TryAdd(
            TowerStatType.CriticalDamage,
            StatModifierType.Flat,
            0.5f), Is.True);
        var stats = new TowerStats(partData, globalModifiers, coreGoldModifiers, null);

        float damage = stats.CalculateAttackDamage(0f, out bool isCritical);

        Assert.That(isCritical, Is.True);
        Assert.That(stats.CriticalDamageMultiplier, Is.EqualTo(2.5f));
        Assert.That(damage, Is.EqualTo(25f));
    }

    private CoreGoldUpgradeData CreateUpgradeData(
        string upgradeId,
        TowerStatType statType,
        float increase,
        int baseCost,
        CoreGoldUpgradeCostGrowth growth,
        float growthValue)
    {
        CoreGoldUpgradeData data = ScriptableObject.CreateInstance<CoreGoldUpgradeData>();
        _createdObjects.Add(data);
        var serializedData = new SerializedObject(data);
        serializedData.FindProperty("_upgradeId").stringValue = upgradeId;
        serializedData.FindProperty("_displayName").stringValue = upgradeId;
        serializedData.FindProperty("_statType").enumValueIndex = (int)statType;
        serializedData.FindProperty("_flatIncreasePerLevel").floatValue = increase;
        serializedData.FindProperty("_baseCost").intValue = baseCost;
        serializedData.FindProperty("_costGrowth").enumValueIndex = (int)growth;
        serializedData.FindProperty("_costGrowthValue").floatValue = growthValue;
        serializedData.ApplyModifiedPropertiesWithoutUndo();
        return data;
    }

    private sealed class FakeUpgradeApplier : ICoreGoldUpgradeApplier
    {
        public bool ApplySucceeds { get; set; } = true;
        public int ApplyCount { get; private set; }

        public bool CanApplyCoreGoldUpgrade(TowerStatType statType, float flatValue)
        {
            return TowerStatModifiers.CanModify(statType, StatModifierType.Flat, flatValue);
        }

        public bool TryApplyCoreGoldUpgrade(TowerStatType statType, float flatValue)
        {
            ApplyCount++;
            return ApplySucceeds;
        }
    }
}
