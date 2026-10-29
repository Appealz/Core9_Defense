using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class RewardSelectionManager
{
    private static readonly RewardRarity[] Rarities =
    {
        RewardRarity.Bronze,
        RewardRarity.Silver,
        RewardRarity.Gold,
        RewardRarity.Platinum
    };

    private readonly IReadOnlyList<RewardData> _rewardDataList;
    private readonly RewardDependencies _dependencies;
    private readonly RewardSelectionSettings _settings;
    private readonly System.Random _random;
    private readonly List<RewardData> _currentChoices = new();

    public IReadOnlyList<RewardData> CurrentChoices => _currentChoices;

    public RewardSelectionManager(
        IReadOnlyList<RewardData> rewardDataList,
        RewardDependencies dependencies,
        RewardSelectionSettings settings,
        System.Random random = null)
    {
        _rewardDataList = rewardDataList ?? throw new ArgumentNullException(nameof(rewardDataList));
        _dependencies = dependencies ?? throw new ArgumentNullException(nameof(dependencies));
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));

        if (!_settings.TryValidate(out string error))
            throw new ArgumentException(error, nameof(settings));

        _random = random ?? new System.Random();
    }

    public IReadOnlyList<RewardData> CreateChoices()
    {
        return CreateChoices(_settings.CardCount);
    }

    public IReadOnlyList<RewardData> CreateChoices(int count)
    {
        int requestedCount = Math.Max(0, count);
        List<RewardData> candidates = CollectValidCandidates();
        _currentChoices.Clear();

        if (requestedCount == 0)
            return _currentChoices;

        int guaranteedCount = Math.Min(_settings.MinimumNewTowerCards, requestedCount);
        List<RewardData> newTowerCandidates = candidates.FindAll(
            rewardData => rewardData.Category == RewardCategory.NewTower);

        AddRarityFirstChoices(newTowerCandidates, guaranteedCount);

        for (int i = 0; i < _currentChoices.Count; i++)
            candidates.Remove(_currentChoices[i]);

        AddRarityFirstChoices(candidates, requestedCount - _currentChoices.Count);

        if (_currentChoices.Count < requestedCount)
        {
            Debug.LogWarning(
                $"Generated {_currentChoices.Count} of {requestedCount} requested reward cards. " +
                "Check CanApply, SO selection weights, and rarity probabilities.");
        }

        Shuffle(_currentChoices);
        return _currentChoices;
    }

    private List<RewardData> CollectValidCandidates()
    {
        List<RewardData> candidates = new();
        HashSet<RewardData> uniqueRewards = new();

        foreach (RewardData rewardData in _rewardDataList)
        {
            if (rewardData == null || !uniqueRewards.Add(rewardData))
                continue;

            if (rewardData.RewardDefinition == null)
            {
                Debug.LogWarning($"Excluded reward without a definition: {rewardData.name}");
                continue;
            }

            if (rewardData.SelectionWeight <= 0f)
                continue;

            IReward reward = rewardData.RewardDefinition.CreateReward(_dependencies);

            if (reward != null && reward.CanApply())
                candidates.Add(rewardData);
        }

        return candidates;
    }

    private void AddRarityFirstChoices(List<RewardData> candidates, int count)
    {
        int remainingCount = Math.Min(count, candidates.Count);

        for (int i = 0; i < remainingCount; i++)
        {
            RewardData selectedReward = TakeRarityFirstRandom(candidates);

            if (selectedReward == null)
                return;

            _currentChoices.Add(selectedReward);
            candidates.Remove(selectedReward);
        }
    }

    private RewardData TakeRarityFirstRandom(IReadOnlyList<RewardData> candidates)
    {
        if (!TrySelectRarity(candidates, out RewardRarity selectedRarity))
            return null;

        double totalWeight = 0d;

        for (int i = 0; i < candidates.Count; i++)
        {
            if (candidates[i].Rarity == selectedRarity)
                totalWeight += candidates[i].SelectionWeight;
        }

        if (totalWeight <= 0d)
            return null;

        double randomValue = _random.NextDouble() * totalWeight;

        for (int i = 0; i < candidates.Count; i++)
        {
            RewardData candidate = candidates[i];

            if (candidate.Rarity != selectedRarity)
                continue;

            randomValue -= candidate.SelectionWeight;

            if (randomValue < 0d)
                return candidate;
        }

        for (int i = candidates.Count - 1; i >= 0; i--)
        {
            if (candidates[i].Rarity == selectedRarity)
                return candidates[i];
        }

        return null;
    }

    private bool TrySelectRarity(
        IReadOnlyList<RewardData> candidates,
        out RewardRarity selectedRarity)
    {
        float totalProbability = 0f;
        RewardRarity lastAvailableRarity = default;
        bool hasAvailableRarity = false;

        for (int i = 0; i < Rarities.Length; i++)
        {
            RewardRarity rarity = Rarities[i];

            if (!HasCandidateOfRarity(candidates, rarity))
                continue;

            float probability = _settings.GetRarityProbability(rarity);

            if (probability <= 0f)
                continue;

            totalProbability += probability;
            lastAvailableRarity = rarity;
            hasAvailableRarity = true;
        }

        if (!hasAvailableRarity || totalProbability <= 0f)
        {
            selectedRarity = default;
            return false;
        }

        double randomValue = _random.NextDouble() * totalProbability;

        for (int i = 0; i < Rarities.Length; i++)
        {
            RewardRarity rarity = Rarities[i];

            if (!HasCandidateOfRarity(candidates, rarity))
                continue;

            float probability = _settings.GetRarityProbability(rarity);

            if (probability <= 0f)
                continue;

            randomValue -= probability;

            if (randomValue < 0d)
            {
                selectedRarity = rarity;
                return true;
            }
        }

        selectedRarity = lastAvailableRarity;
        return true;
    }

    private static bool HasCandidateOfRarity(
        IReadOnlyList<RewardData> candidates,
        RewardRarity rarity)
    {
        for (int i = 0; i < candidates.Count; i++)
        {
            if (candidates[i].Rarity == rarity && candidates[i].SelectionWeight > 0f)
                return true;
        }

        return false;
    }

    public async UniTask<bool> ApplyAsync(RewardData rewardData)
    {
        if (!_currentChoices.Contains(rewardData))
            throw new InvalidOperationException("The reward is not in the current choices.");

        IReward reward = rewardData.RewardDefinition.CreateReward(_dependencies);

        if (!reward.CanApply())
            throw new InvalidOperationException("The selected reward can no longer be applied.");

        bool isApplied = await reward.ApplyAsync();

        if (!isApplied)
            throw new InvalidOperationException($"Failed to apply reward: {rewardData.RewardName}");

        _currentChoices.Clear();
        return true;
    }

#if UNITY_EDITOR
    public async UniTask<bool> ApplyForDebugAsync(RewardData rewardData)
    {
        if (rewardData == null || rewardData.RewardDefinition == null)
        {
            Debug.LogWarning("[Combat Debug] RewardData or RewardDefinition is missing.");
            return false;
        }

        IReward reward = rewardData.RewardDefinition.CreateReward(_dependencies);

        if (reward == null || !reward.CanApply())
        {
            Debug.LogWarning($"[Combat Debug] Reward cannot be applied: {rewardData.RewardName}");
            return false;
        }

        bool isApplied = await reward.ApplyAsync();

        if (isApplied)
            Debug.Log($"[Combat Debug] Applied reward through the real reward path: {rewardData.RewardName}");
        else
            Debug.LogWarning($"[Combat Debug] Reward application failed: {rewardData.RewardName}");

        return isApplied;
    }
#endif

    private void Shuffle(List<RewardData> rewards)
    {
        for (int i = rewards.Count - 1; i > 0; i--)
        {
            int randomIndex = _random.Next(i + 1);
            RewardData temp = rewards[i];
            rewards[i] = rewards[randomIndex];
            rewards[randomIndex] = temp;
        }
    }
}
