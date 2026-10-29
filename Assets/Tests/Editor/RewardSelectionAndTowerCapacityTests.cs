using System;
using System.Collections.Generic;
using System.Reflection;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;

public sealed class RewardSelectionAndTowerCapacityTests
{
    private readonly List<RewardData> _createdRewards = new();

    [TearDown]
    public void TearDown()
    {
        for (int i = 0; i < _createdRewards.Count; i++)
            UnityEngine.Object.DestroyImmediate(_createdRewards[i]);

        _createdRewards.Clear();
    }

    [TestCase(1, 2)]
    [TestCase(4, 2)]
    [TestCase(5, 3)]
    [TestCase(9, 3)]
    [TestCase(10, 4)]
    [TestCase(15, 5)]
    [TestCase(35, 9)]
    public void TowerCapacity_ReturnsExpectedLimit(int waveNumber, int expectedLimit)
    {
        Assert.That(TowerCapacityPolicy.GetMaxTowerCount(waveNumber), Is.EqualTo(expectedLimit));
    }

    [Test]
    public void DefaultRarityProbabilities_PreservePreviousWeightRatios()
    {
        var settings = new RewardSelectionSettings();

        Assert.That(settings.TryValidate(out string error), Is.True, error);
        Assert.That(settings.GetRarityProbability(RewardRarity.Bronze), Is.EqualTo(47.61905f).Within(0.001f));
        Assert.That(settings.GetRarityProbability(RewardRarity.Silver), Is.EqualTo(30.95238f).Within(0.001f));
        Assert.That(settings.GetRarityProbability(RewardRarity.Gold), Is.EqualTo(16.66667f).Within(0.001f));
        Assert.That(settings.GetRarityProbability(RewardRarity.Platinum), Is.EqualTo(4.7619f).Within(0.001f));
    }

    [Test]
    public void InvalidRarityProbabilityTotal_IsRejected()
    {
        RewardSelectionSettings settings = CreateSettings(50f, 30f, 10f, 5f, 0);

        Assert.That(settings.TryValidate(out string error), Is.False);
        Assert.That(error, Does.Contain("100%"));
        Assert.Throws<ArgumentException>(() => CreateManager(Array.Empty<RewardData>(), settings, 1));
    }

    [Test]
    public void UnavailableNewTower_IsExcludedFromGuaranteeAndGeneralDraws()
    {
        MutableRewardDefinition unavailableDefinition = new MutableRewardDefinition(false);
        RewardData unavailableNewTower = CreateReward(
            "Unavailable Tower",
            RewardRarity.Bronze,
            RewardCategory.NewTower,
            1f,
            unavailableDefinition);
        List<RewardData> rewards = new List<RewardData> { unavailableNewTower };

        for (int i = 0; i < 3; i++)
        {
            rewards.Add(CreateReward(
                $"Standard {i}",
                RewardRarity.Bronze,
                RewardCategory.Standard,
                1f,
                new MutableRewardDefinition(true)));
        }

        RewardSelectionManager manager = CreateManager(
            rewards,
            CreateSettings(100f, 0f, 0f, 0f, 1),
            2);
        IReadOnlyList<RewardData> choices = manager.CreateChoices(3);

        Assert.That(choices, Has.Count.EqualTo(3));
        Assert.That(ContainsReference(choices, unavailableNewTower), Is.False);
    }

    [Test]
    public void NewTower_ReappearsAfterCapacityBecomesAvailableAgain()
    {
        MutableRewardDefinition towerDefinition = new MutableRewardDefinition(true);
        RewardData newTower = CreateReward(
            "Tower",
            RewardRarity.Bronze,
            RewardCategory.NewTower,
            1f,
            towerDefinition);
        RewardData standard = CreateReward(
            "Standard",
            RewardRarity.Bronze,
            RewardCategory.Standard,
            1f,
            new MutableRewardDefinition(true));
        RewardSelectionManager manager = CreateManager(
            new[] { newTower, standard },
            CreateSettings(100f, 0f, 0f, 0f, 1),
            3);

        Assert.That(manager.CreateChoices(1), Does.Contain(newTower));

        towerDefinition.CanApplyValue = false;
        Assert.That(ContainsReference(manager.CreateChoices(1), newTower), Is.False);

        towerDefinition.CanApplyValue = true;
        Assert.That(manager.CreateChoices(1), Does.Contain(newTower));
    }

    [Test]
    public void EmptyRarity_IsExcludedAndRemainingRarityIsRenormalized()
    {
        RewardData silver = CreateReward(
            "Silver",
            RewardRarity.Silver,
            RewardCategory.Standard,
            1f,
            new MutableRewardDefinition(true));
        RewardSelectionManager manager = CreateManager(
            new[] { silver },
            CreateSettings(80f, 20f, 0f, 0f, 0),
            4);

        for (int i = 0; i < 100; i++)
            Assert.That(manager.CreateChoices(1)[0].Rarity, Is.EqualTo(RewardRarity.Silver));
    }

    [Test]
    public void RarityProbability_DoesNotDependOnCandidateCountWithinRarity()
    {
        RewardSelectionSettings settings = CreateSettings(60f, 40f, 0f, 0f, 0);
        List<RewardData> compactPool = CreateRarityPool(1, 1);
        List<RewardData> expandedPool = CreateRarityPool(10, 1);
        float compactSilverRate = MeasureRarityRate(compactPool, settings, RewardRarity.Silver, 10000, 10);
        float expandedSilverRate = MeasureRarityRate(expandedPool, settings, RewardRarity.Silver, 10000, 10);

        Assert.That(compactSilverRate, Is.EqualTo(0.4f).Within(0.02f));
        Assert.That(expandedSilverRate, Is.EqualTo(0.4f).Within(0.02f));
        Assert.That(expandedSilverRate, Is.EqualTo(compactSilverRate).Within(0.02f));
    }

    [Test]
    public void SelectionWeight_ControlsRatioOnlyInsideSelectedRarity()
    {
        RewardData light = CreateReward(
            "Light",
            RewardRarity.Bronze,
            RewardCategory.Standard,
            1f,
            new MutableRewardDefinition(true));
        RewardData heavy = CreateReward(
            "Heavy",
            RewardRarity.Bronze,
            RewardCategory.Standard,
            3f,
            new MutableRewardDefinition(true));
        RewardSelectionManager manager = CreateManager(
            new[] { light, heavy },
            CreateSettings(100f, 0f, 0f, 0f, 0),
            11);
        int heavyCount = 0;
        const int drawCount = 10000;

        for (int i = 0; i < drawCount; i++)
        {
            if (manager.CreateChoices(1)[0] == heavy)
                heavyCount++;
        }

        Assert.That((float)heavyCount / drawCount, Is.EqualTo(0.75f).Within(0.02f));
    }

    [Test]
    public void ThreeCardSelection_RemovesDuplicatesAndZeroWeightCandidates()
    {
        RewardData zeroWeight = CreateReward(
            "Zero",
            RewardRarity.Bronze,
            RewardCategory.Standard,
            0f,
            new MutableRewardDefinition(true));
        List<RewardData> rewards = new List<RewardData> { zeroWeight };

        for (int i = 0; i < 3; i++)
        {
            rewards.Add(CreateReward(
                $"Valid {i}",
                RewardRarity.Bronze,
                RewardCategory.Standard,
                1f,
                new MutableRewardDefinition(true)));
        }

        RewardSelectionManager manager = CreateManager(
            rewards,
            CreateSettings(100f, 0f, 0f, 0f, 0),
            12);
        IReadOnlyList<RewardData> choices = manager.CreateChoices(3);

        Assert.That(choices, Has.Count.EqualTo(3));
        Assert.That(new HashSet<RewardData>(choices), Has.Count.EqualTo(3));
        Assert.That(ContainsReference(choices, zeroWeight), Is.False);
    }

    private List<RewardData> CreateRarityPool(int bronzeCount, int silverCount)
    {
        List<RewardData> rewards = new List<RewardData>();

        for (int i = 0; i < bronzeCount; i++)
        {
            rewards.Add(CreateReward(
                $"Bronze {i}",
                RewardRarity.Bronze,
                RewardCategory.Standard,
                1f,
                new MutableRewardDefinition(true)));
        }

        for (int i = 0; i < silverCount; i++)
        {
            rewards.Add(CreateReward(
                $"Silver {i}",
                RewardRarity.Silver,
                RewardCategory.Standard,
                1f,
                new MutableRewardDefinition(true)));
        }

        return rewards;
    }

    private static bool ContainsReference(
        IReadOnlyList<RewardData> rewards,
        RewardData target)
    {
        for (int i = 0; i < rewards.Count; i++)
        {
            if (ReferenceEquals(rewards[i], target))
                return true;
        }

        return false;
    }

    private static float MeasureRarityRate(
        IReadOnlyList<RewardData> rewards,
        RewardSelectionSettings settings,
        RewardRarity rarity,
        int drawCount,
        int seed)
    {
        RewardSelectionManager manager = CreateManager(rewards, settings, seed);
        int matchingCount = 0;

        for (int i = 0; i < drawCount; i++)
        {
            if (manager.CreateChoices(1)[0].Rarity == rarity)
                matchingCount++;
        }

        return (float)matchingCount / drawCount;
    }

    private RewardData CreateReward(
        string rewardName,
        RewardRarity rarity,
        RewardCategory category,
        float selectionWeight,
        MutableRewardDefinition definition)
    {
        RewardData rewardData = ScriptableObject.CreateInstance<RewardData>();
        SetField(rewardData, "_rewardName", rewardName);
        SetField(rewardData, "_rarity", rarity);
        SetField(rewardData, "_category", category);
        SetField(rewardData, "_selectionWeight", selectionWeight);
        SetField(rewardData, "_rewardDefinition", definition);
        _createdRewards.Add(rewardData);
        return rewardData;
    }

    private static RewardSelectionSettings CreateSettings(
        float bronze,
        float silver,
        float gold,
        float platinum,
        int minimumNewTowerCards)
    {
        var settings = new RewardSelectionSettings();
        SetField(settings, "_bronzeProbability", bronze);
        SetField(settings, "_silverProbability", silver);
        SetField(settings, "_goldProbability", gold);
        SetField(settings, "_platinumProbability", platinum);
        SetField(settings, "_minimumNewTowerCards", minimumNewTowerCards);
        return settings;
    }

    private static RewardSelectionManager CreateManager(
        IReadOnlyList<RewardData> rewards,
        RewardSelectionSettings settings,
        int seed)
    {
        return new RewardSelectionManager(
            rewards,
            new RewardDependencies(null, null, null),
            settings,
            new System.Random(seed));
    }

    private static void SetField(object target, string fieldName, object value)
    {
        FieldInfo field = target.GetType().GetField(
            fieldName,
            BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.That(field, Is.Not.Null, $"Missing field {fieldName}");
        field.SetValue(target, value);
    }

    [Serializable]
    private sealed class MutableRewardDefinition : RewardDefinition
    {
        public bool CanApplyValue { get; set; }

        public MutableRewardDefinition(bool canApplyValue)
        {
            CanApplyValue = canApplyValue;
        }

        public override IReward CreateReward(RewardDependencies dependencies)
        {
            return new MutableReward(this);
        }
    }

    private sealed class MutableReward : IReward
    {
        private readonly MutableRewardDefinition _definition;

        public MutableReward(MutableRewardDefinition definition)
        {
            _definition = definition;
        }

        public bool CanApply()
        {
            return _definition.CanApplyValue;
        }

        public UniTask<bool> ApplyAsync()
        {
            return UniTask.FromResult(CanApply());
        }
    }
}
