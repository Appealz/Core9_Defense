using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public sealed class TowerStatSummaryTests
{
    private readonly List<Object> _createdObjects = new();

    [TearDown]
    public void TearDown()
    {
        for (int i = _createdObjects.Count - 1; i >= 0; i--)
        {
            if (_createdObjects[i] != null)
                Object.DestroyImmediate(_createdObjects[i]);
        }

        _createdObjects.Clear();
    }

    [Test]
    public void Summary_WithoutModifiers_UsesBaseValuesForEveryStage()
    {
        TowerStats stats = CreateStats(10f, 0.5f, 100);
        TowerStatSummary summary = stats.CreateStatSummary(
            new TowerStatModifiers(),
            new TowerStatModifiers(),
            new TowerStatModifiers(),
            new TowerStatModifiers());

        AssertValues(summary.Base, 10f, 2f, 100f);
        AssertValues(summary.NonCore, 10f, 2f, 100f);
        AssertValues(summary.Final, 10f, 2f, 100f);
    }

    [Test]
    public void Summary_FlatAndPercentSources_UsesCombatFormulaForBothDeltas()
    {
        TowerPartData data = CreatePartData(10f, 0.5f, 100);
        var global = new TowerStatModifiers();
        var coreGold = new TowerStatModifiers();
        var type = new TowerStatModifiers();
        var individual = new TowerStatModifiers();
        var slot = new TowerStatModifiers();

        global.TryAdd(TowerStatType.AttackDamage, StatModifierType.Flat, 2f);
        global.TryAdd(TowerStatType.AttackDamage, StatModifierType.Percent, 0.1f);
        coreGold.TryAdd(TowerStatType.AttackDamage, StatModifierType.Flat, 3f);
        type.TryAdd(TowerStatType.AttackDamage, StatModifierType.Percent, 0.2f);
        individual.TryAdd(TowerStatType.AttackDamage, StatModifierType.Flat, 5f);
        slot.TryAdd(TowerStatType.AttackDamage, StatModifierType.Flat, 4f);
        slot.TryAdd(TowerStatType.AttackDamage, StatModifierType.Percent, 0.25f);

        var stats = new TowerStats(data, global, coreGold, type);
        stats.ApplyModifiers(global, coreGold, slot, type, individual);
        TowerStatSummary summary = stats.CreateStatSummary(global, coreGold, type, individual);

        Assert.That(summary.Base.AttackDamage, Is.EqualTo(10f).Within(0.0001f));
        Assert.That(summary.NonCore.AttackDamage, Is.EqualTo(26f).Within(0.0001f));
        Assert.That(summary.Final.AttackDamage, Is.EqualTo(37.2f).Within(0.0001f));
        Assert.That(summary.NonCore.AttackDamage - summary.Base.AttackDamage,
            Is.EqualTo(16f).Within(0.0001f));
        Assert.That(summary.Final.AttackDamage - summary.NonCore.AttackDamage,
            Is.EqualTo(11.2f).Within(0.0001f));
    }

    [Test]
    public void Summary_DifferentSlotEffects_ProducesDifferentCoreContributionOnly()
    {
        TowerPartData data = CreatePartData(10f, 0.5f, 100);
        var global = new TowerStatModifiers();
        var coreGold = new TowerStatModifiers();
        var type = new TowerStatModifiers();
        var individual = new TowerStatModifiers();
        var firstSlot = new TowerStatModifiers();
        var secondSlot = new TowerStatModifiers();
        global.TryAdd(TowerStatType.AttackDamage, StatModifierType.Flat, 5f);
        firstSlot.TryAdd(TowerStatType.AttackDamage, StatModifierType.Flat, 3f);
        secondSlot.TryAdd(TowerStatType.AttackDamage, StatModifierType.Percent, 0.5f);

        var firstStats = new TowerStats(data, global, coreGold, type);
        var secondStats = new TowerStats(data, global, coreGold, type);
        firstStats.ApplyModifiers(global, coreGold, firstSlot, type, individual);
        secondStats.ApplyModifiers(global, coreGold, secondSlot, type, individual);

        TowerStatSummary first = firstStats.CreateStatSummary(global, coreGold, type, individual);
        TowerStatSummary second = secondStats.CreateStatSummary(global, coreGold, type, individual);

        Assert.That(first.NonCore.AttackDamage, Is.EqualTo(second.NonCore.AttackDamage));
        Assert.That(first.Final.AttackDamage, Is.EqualTo(18f).Within(0.0001f));
        Assert.That(second.Final.AttackDamage, Is.EqualTo(22.5f).Within(0.0001f));
    }

    [Test]
    public void Summary_AttackSpeedAndMaxHp_UseTheSameFlatPercentRules()
    {
        TowerPartData data = CreatePartData(10f, 0.5f, 100);
        var global = new TowerStatModifiers();
        var coreGold = new TowerStatModifiers();
        var type = new TowerStatModifiers();
        var individual = new TowerStatModifiers();
        var slot = new TowerStatModifiers();

        global.TryAdd(TowerStatType.AttackSpeed, StatModifierType.Percent, 0.5f);
        individual.TryAdd(TowerStatType.AttackSpeed, StatModifierType.Flat, 0.5f);
        slot.TryAdd(TowerStatType.AttackSpeed, StatModifierType.Flat, 0.25f);
        slot.TryAdd(TowerStatType.AttackSpeed, StatModifierType.Percent, 0.1f);
        coreGold.TryAdd(TowerStatType.MaxHp, StatModifierType.Flat, 20f);
        type.TryAdd(TowerStatType.MaxHp, StatModifierType.Percent, 0.1f);
        slot.TryAdd(TowerStatType.MaxHp, StatModifierType.Percent, 0.25f);

        var stats = new TowerStats(data, global, coreGold, type);
        stats.ApplyModifiers(global, coreGold, slot, type, individual);
        TowerStatSummary summary = stats.CreateStatSummary(global, coreGold, type, individual);

        Assert.That(summary.NonCore.AttackSpeed, Is.EqualTo(3.75f).Within(0.0001f));
        Assert.That(summary.Final.AttackSpeed, Is.EqualTo(4.4f).Within(0.0001f));
        Assert.That(summary.NonCore.MaxHp, Is.EqualTo(132f).Within(0.0001f));
        Assert.That(summary.Final.MaxHp, Is.EqualTo(162f).Within(0.0001f));
    }

    [Test]
    public void TowerManager_SlotChangeAndMove_RaisesStatsChangedWithUpdatedSummary()
    {
        TowerPartData data = CreatePartData(10f, 0.5f, 100);
        var global = new TowerStatModifiers();
        var coreGold = new TowerStatModifiers();
        var typeState = new TowerTypeUpgradeState(null);
        var board = new CoreBoard();
        var coreManager = new CoreManager(board);
        GameObject layoutObject = new("SummaryLayout");
        _createdObjects.Add(layoutObject);
        CoreLayout layout = layoutObject.AddComponent<CoreLayout>();
        using var manager = new TowerManager(
            null,
            coreManager,
            layout,
            global,
            coreGold,
            typeState);
        Tower tower = CreateTower(data, global, coreGold, typeState);

        Assert.That(coreManager.TryPlaceTower(1, tower), Is.True);
        GetActiveTowers(manager).Add(tower);
        int changedCount = 0;
        manager.TowerStatsChanged += changedTower =>
        {
            if (changedTower == tower)
                changedCount++;
        };

        var flatEffect = new StatCoreSlotEffect(
            "summary_flat",
            1,
            Color.white,
            0,
            TowerStatType.AttackDamage,
            StatModifierType.Flat,
            3f,
            3f);
        var percentEffect = new StatCoreSlotEffect(
            "summary_percent",
            1,
            Color.white,
            0,
            TowerStatType.AttackDamage,
            StatModifierType.Percent,
            0.5f,
            0.5f);

        Assert.That(coreManager.TryAddSlotEffects(new[] { 1 }, new[] { flatEffect }), Is.True);
        Assert.That(manager.TryGetStatSummary(tower, out TowerStatSummary beforeMove), Is.True);
        Assert.That(beforeMove.Final.AttackDamage, Is.EqualTo(13f).Within(0.0001f));
        Assert.That(coreManager.TryAddSlotEffects(new[] { 2 }, new[] { percentEffect }), Is.True);
        Assert.That(manager.TryMoveOrSwapTower(1, 2), Is.True);
        Assert.That(manager.TryGetStatSummary(tower, out TowerStatSummary afterMove), Is.True);
        Assert.That(afterMove.Final.AttackDamage, Is.EqualTo(15f).Within(0.0001f));
        Assert.That(changedCount, Is.EqualTo(2));
    }

    [Test]
    public void BattleScene_StatSummaryIsCompactHorizontalAndConnected()
    {
        EditorSceneManager.OpenScene("Assets/Scenes/Battle.unity", OpenSceneMode.Single);
        GameObject panel = FindInactiveGameObject("TowerGoldUpgradePanel");
        Transform summaryTransform = panel.transform.Find("StatSummaryText");
        Transform legendTransform = panel.transform.Find("StatLegendText");
        Transform viewportTransform = panel.transform.Find("Viewport");

        Assert.That(panel.GetComponent<TowerGoldUpgradeView>().IsConfigured, Is.True);
        Assert.That(summaryTransform, Is.Not.Null);
        Assert.That(legendTransform, Is.Not.Null);
        Assert.That(viewportTransform, Is.Not.Null);

        TMP_Text summaryText = summaryTransform.GetComponent<TMP_Text>();
        RectTransform summaryRect = summaryTransform.GetComponent<RectTransform>();
        RectTransform legendRect = legendTransform.GetComponent<RectTransform>();
        RectTransform panelRect = panel.GetComponent<RectTransform>();
        RectTransform viewportRect = viewportTransform.GetComponent<RectTransform>();
        CanvasScaler canvasScaler = panel.GetComponentInParent<Canvas>().GetComponent<CanvasScaler>();

        Assert.That(summaryText.enableAutoSizing, Is.True);
        Assert.That(summaryText.text, Does.Not.Contain("\n"));
        Assert.That(summaryRect.anchorMin.x, Is.Zero);
        Assert.That(summaryRect.anchorMax.x, Is.EqualTo(1f));
        Assert.That(legendRect.anchorMin, Is.EqualTo(Vector2.one));
        Assert.That(legendRect.pivot, Is.EqualTo(Vector2.one));
        Assert.That(panelRect.anchorMax.y, Is.EqualTo(0.25f));
        Assert.That(viewportRect.sizeDelta.y, Is.EqualTo(-128f));
        Assert.That(canvasScaler.uiScaleMode, Is.EqualTo(CanvasScaler.ScaleMode.ScaleWithScreenSize));
        Assert.That(canvasScaler.matchWidthOrHeight, Is.Zero);

        panel.SetActive(true);
        summaryText.text =
            "ATK <color=#FFFFFF>25</color> <color=#FFA43B>(+12)</color> <color=#61D17C>(+8)</color>" +
            "    ASPD <color=#FFFFFF>1.5/s</color> <color=#FFA43B>(+0.3)</color> <color=#61D17C>(+0.2)</color>" +
            "    MAX HP <color=#FFFFFF>80</color> <color=#FFA43B>(+30)</color>";
        Canvas.ForceUpdateCanvases();
        summaryText.ForceMeshUpdate(true);

        Assert.That(summaryText.textInfo.lineCount, Is.EqualTo(1));
        Assert.That(summaryText.isTextOverflowing, Is.False);
        Assert.That(summaryText.fontSize, Is.GreaterThanOrEqualTo(summaryText.fontSizeMin));
    }

    private TowerStats CreateStats(float attackDamage, float attackRate, int maxHp)
    {
        TowerPartData data = CreatePartData(attackDamage, attackRate, maxHp);
        return new TowerStats(
            data,
            new TowerStatModifiers(),
            new TowerStatModifiers(),
            new TowerStatModifiers());
    }

    private TowerPartData CreatePartData(float attackDamage, float attackRate, int maxHp)
    {
        TowerPartData data = ScriptableObject.CreateInstance<TowerPartData>();
        _createdObjects.Add(data);
        var serializedData = new SerializedObject(data);
        serializedData.FindProperty("_towerType").enumValueIndex = (int)TowerType.Basic;
        serializedData.FindProperty("_towerName").stringValue = "Summary Tower";
        serializedData.FindProperty("_maxHp").intValue = maxHp;
        serializedData.FindProperty("_attackDamage").floatValue = attackDamage;
        serializedData.FindProperty("_attackRange").floatValue = 5f;
        serializedData.FindProperty("_attackRate").floatValue = attackRate;
        serializedData.FindProperty("_minimumAttackInterval").floatValue = 0.05f;
        serializedData.FindProperty("_criticalDamageMultiplier").floatValue = 2f;
        serializedData.FindProperty("_criticalChanceCap").floatValue = 1f;
        serializedData.ApplyModifiedPropertiesWithoutUndo();
        return data;
    }

    private Tower CreateTower(
        TowerPartData data,
        TowerStatModifiers global,
        TowerStatModifiers coreGold,
        TowerTypeUpgradeState typeState)
    {
        GameObject towerObject = new("SummaryTower");
        _createdObjects.Add(towerObject);
        towerObject.AddComponent<SpriteRenderer>();
        towerObject.AddComponent<TowerHitFlash>();
        Tower tower = towerObject.AddComponent<Tower>();
        typeof(Tower).GetMethod("Awake", BindingFlags.Instance | BindingFlags.NonPublic)
            ?.Invoke(tower, null);
        TowerStats stats = new(
            data,
            global,
            coreGold,
            typeState.GetStatModifiers(TowerType.Basic));
        tower.Initialize(
            new TowerHealth(stats.MaxHp),
            stats,
            null,
            null,
            TowerType.Basic,
            null,
            data.TowerName);
        return tower;
    }

    private static List<Tower> GetActiveTowers(TowerManager manager)
    {
        return (List<Tower>)typeof(TowerManager)
            .GetField("_activeTowers", BindingFlags.Instance | BindingFlags.NonPublic)
            .GetValue(manager);
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

    private static void AssertValues(
        TowerStatValues values,
        float attackDamage,
        float attackSpeed,
        float maxHp)
    {
        Assert.That(values.AttackDamage, Is.EqualTo(attackDamage).Within(0.0001f));
        Assert.That(values.AttackSpeed, Is.EqualTo(attackSpeed).Within(0.0001f));
        Assert.That(values.MaxHp, Is.EqualTo(maxHp).Within(0.0001f));
    }
}
