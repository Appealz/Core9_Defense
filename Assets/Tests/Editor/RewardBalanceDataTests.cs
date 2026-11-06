using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEditor;

public sealed class RewardBalanceDataTests
{
    private const string RewardRoot = "Assets/Script/Reward/SO";
    private const string GameStartDataPath = "Assets/Script/Game/GameStartData.asset";

    [Test]
    public void RewardCatalog_RegistersAllFortyOneRewardAssetsExactlyOnce()
    {
        GameStartData gameStartData = AssetDatabase.LoadAssetAtPath<GameStartData>(GameStartDataPath);
        Assert.That(gameStartData, Is.Not.Null);

        RewardData[] allRewardAssets = AssetDatabase.FindAssets("t:RewardData", new[] { RewardRoot })
            .Select(AssetDatabase.GUIDToAssetPath)
            .Select(AssetDatabase.LoadAssetAtPath<RewardData>)
            .Where(reward => reward != null)
            .ToArray();

        Assert.That(allRewardAssets, Has.Length.EqualTo(41));
        Assert.That(gameStartData.Rewards.Rewards, Has.Count.EqualTo(41));
        Assert.That(gameStartData.Rewards.Rewards.Distinct().Count(), Is.EqualTo(41));
        CollectionAssert.AreEquivalent(allRewardAssets, gameStartData.Rewards.Rewards);

        Assert.That(gameStartData.Rewards.Rewards.Count(reward => reward.Category == RewardCategory.NewTower),
            Is.EqualTo(6));
        Assert.That(gameStartData.Rewards.Rewards.Count(reward => reward.Category == RewardCategory.GlobalStat),
            Is.EqualTo(7));
        Assert.That(gameStartData.Rewards.Rewards.Count(reward => reward.Category == RewardCategory.CoreSlot),
            Is.EqualTo(7));
        Assert.That(gameStartData.Rewards.Rewards.Count(reward => reward.Category == RewardCategory.TowerType),
            Is.EqualTo(18));
        Assert.That(gameStartData.Rewards.Rewards.Count(reward => reward.Category == RewardCategory.Gold),
            Is.EqualTo(3));
    }

    [Test]
    public void UpdatedRewards_HaveFinalBalanceValues()
    {
        AssertReward(
            "GlobalStat/MaxHpFlatReward.asset",
            RewardRarity.Bronze,
            RewardCategory.GlobalStat,
            1f,
            "Max HP +10");
        AssertReward(
            "GlobalStat/AttackSpeedPercentReward.asset",
            RewardRarity.Silver,
            RewardCategory.GlobalStat,
            1f,
            "Attack Speed +10%");
        AssertReward(
            "GlobalStat/MaxHpPercentReward.asset",
            RewardRarity.Silver,
            RewardCategory.GlobalStat,
            1f,
            "Max HP +10%");
        AssertReward(
            "GlobalStat/AttackDamagePercentReward.asset",
            RewardRarity.Gold,
            RewardCategory.GlobalStat,
            1f,
            "Attack Damage +5%");
        AssertReward(
            "GlobalStat/AttackDamageFlatReward_Bronze.asset",
            RewardRarity.Bronze,
            RewardCategory.GlobalStat,
            1f,
            "Attack Damage +5");
        AssertReward(
            "CoreSlot/CoreSlotRegenerationDiagonalReward.asset",
            RewardRarity.Platinum,
            RewardCategory.CoreSlot,
            1f,
            "Diagonal Cross: Regenerate 2 HP + 1% Max HP every 1s");
        AssertReward(
            "CoreSlot/CoreSlotDamageGoldCrossReward.asset",
            RewardRarity.Platinum,
            RewardCategory.CoreSlot,
            1f,
            "Cross: Attack Damage +10%");
        AssertReward(
            "CoreSlot/CoreSlotSpeedCrossReward.asset",
            RewardRarity.Platinum,
            RewardCategory.CoreSlot,
            1f,
            "Cross: Attack Speed +10%");
        AssertReward(
            "CoreSlot/CoreSlotSpeedReverseLReward.asset",
            RewardRarity.Gold,
            RewardCategory.CoreSlot,
            0.9f,
            "Reverse L Shape: Attack Speed +5%");
        AssertReward(
            "CoreSlot/CoreSlotSpeedSingleReward.asset",
            RewardRarity.Bronze,
            RewardCategory.CoreSlot,
            1f,
            "Single: Attack Speed +5%");
        AssertReward(
            "NewTower/BombTowerReward.asset",
            RewardRarity.Gold,
            RewardCategory.NewTower,
            0.8f,
            null);
    }

    [Test]
    public void GoldRewards_HaveExpectedRarityWeightAndAmount()
    {
        AssertReward(
            "Gold/GoldData.asset",
            RewardRarity.Bronze,
            RewardCategory.Gold,
            1f,
            "Gold +100");
        AssertReward(
            "Gold/GoldData_Silver.asset",
            RewardRarity.Silver,
            RewardCategory.Gold,
            1f,
            "Gold +200");
        AssertReward(
            "Gold/GoldData_Gold.asset",
            RewardRarity.Gold,
            RewardCategory.Gold,
            1f,
            "Gold +400");
    }

    [Test]
    public void AttackSpeedPercentEffects_ShareExistingStackingRules()
    {
        StatCoreSlotEffect cross = LoadEffect("SlotAttackSpeedPercentEffect.asset").CreateEffect()
            as StatCoreSlotEffect;
        StatCoreSlotEffect bronze = LoadEffect("SlotAttackSpeedPercentBronzeEffect.asset").CreateEffect()
            as StatCoreSlotEffect;
        StatCoreSlotEffect gold = LoadEffect("SlotAttackSpeedPercentGoldEffect.asset").CreateEffect()
            as StatCoreSlotEffect;

        AssertEffect(cross, 0.1f);
        AssertEffect(bronze, 0.05f);
        AssertEffect(gold, 0.05f);
    }

    [Test]
    public void NewAssets_HaveDistinctGuids()
    {
        string[] paths =
        {
            "Assets/Script/Core/Enhancement/SlotAttackSpeedPercentBronzeEffect.asset",
            "Assets/Script/Core/Enhancement/SlotAttackSpeedPercentGoldEffect.asset",
            $"{RewardRoot}/CoreSlot/CoreSlotSpeedSingleReward.asset",
            $"{RewardRoot}/Gold/GoldData_Silver.asset",
            $"{RewardRoot}/Gold/GoldData_Gold.asset"
        };

        string[] guids = paths.Select(AssetDatabase.AssetPathToGUID).ToArray();
        Assert.That(guids, Has.All.Not.Empty);
        Assert.That(guids.Distinct().Count(), Is.EqualTo(paths.Length));
    }

    private static RewardData LoadReward(string relativePath)
    {
        RewardData reward = AssetDatabase.LoadAssetAtPath<RewardData>($"{RewardRoot}/{relativePath}");
        Assert.That(reward, Is.Not.Null, relativePath);
        return reward;
    }

    private static StatCoreSlotEffectDefinition LoadEffect(string fileName)
    {
        StatCoreSlotEffectDefinition effect = AssetDatabase.LoadAssetAtPath<StatCoreSlotEffectDefinition>(
            $"Assets/Script/Core/Enhancement/{fileName}");
        Assert.That(effect, Is.Not.Null, fileName);
        return effect;
    }

    private static void AssertReward(
        string relativePath,
        RewardRarity rarity,
        RewardCategory category,
        float selectionWeight,
        string description)
    {
        RewardData reward = LoadReward(relativePath);
        Assert.That(reward.Rarity, Is.EqualTo(rarity), relativePath);
        Assert.That(reward.Category, Is.EqualTo(category), relativePath);
        Assert.That(reward.SelectionWeight, Is.EqualTo(selectionWeight).Within(0.0001f), relativePath);
        Assert.That(reward.RewardDefinition, Is.Not.Null, relativePath);

        if (description != null)
            Assert.That(reward.Description, Is.EqualTo(description), relativePath);
    }

    private static void AssertEffect(StatCoreSlotEffect effect, float expectedValue)
    {
        Assert.That(effect, Is.Not.Null);
        Assert.That(effect.EffectId, Is.EqualTo("slot_attack_speed_percent"));
        Assert.That(effect.MaxStackCount, Is.EqualTo(4));
        Assert.That(effect.StatType, Is.EqualTo(TowerStatType.AttackSpeed));
        Assert.That(effect.ModifierType, Is.EqualTo(StatModifierType.Percent));
        Assert.That(effect.Value, Is.EqualTo(expectedValue).Within(0.0001f));
        Assert.That(effect.MaxTotalValue, Is.EqualTo(0.8f).Within(0.0001f));
    }
}
