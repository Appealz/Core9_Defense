
using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class RewardSelectionManager
{
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
            rewardData => rewardData.Category == RewardCategory.NewTower
        );

        AddWeightedChoices(newTowerCandidates, guaranteedCount);

        foreach (RewardData selectedReward in _currentChoices)
            candidates.Remove(selectedReward);

        AddWeightedChoices(candidates, requestedCount - _currentChoices.Count);

        if (_currentChoices.Count < requestedCount)
        {
            Debug.LogWarning(
                $"요청한 보상 카드 {requestedCount}개 중 {_currentChoices.Count}개만 생성했습니다. " +
                "CanApply, 선택 가중치, 희귀도 가중치 및 보상 데이터 구성을 확인하세요."
            );
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
                Debug.LogWarning($"RewardDefinition이 없는 보상 데이터를 제외했습니다: {rewardData.name}");
                continue;
            }

            if (GetFinalWeight(rewardData) <= 0d)
                continue;

            IReward reward = rewardData.RewardDefinition.CreateReward(_dependencies);

            if (reward != null && reward.CanApply())
                candidates.Add(rewardData);
        }

        return candidates;
    }

    private void AddWeightedChoices(List<RewardData> candidates, int count)
    {
        int remainingCount = Math.Min(count, candidates.Count);

        for (int i = 0; i < remainingCount; i++)
        {
            RewardData selectedReward = TakeWeightedRandom(candidates);

            if (selectedReward == null)
                return;

            _currentChoices.Add(selectedReward);
            candidates.Remove(selectedReward);
        }
    }

    private RewardData TakeWeightedRandom(IReadOnlyList<RewardData> candidates)
    {
        double totalWeight = 0d;

        for (int i = 0; i < candidates.Count; i++)
            totalWeight += GetFinalWeight(candidates[i]);

        if (totalWeight <= 0d)
            return null;

        double randomValue = _random.NextDouble() * totalWeight;

        for (int i = 0; i < candidates.Count; i++)
        {
            randomValue -= GetFinalWeight(candidates[i]);

            if (randomValue < 0d)
                return candidates[i];
        }

        return candidates[candidates.Count - 1];
    }

    private double GetFinalWeight(RewardData rewardData)
    {
        return rewardData.SelectionWeight * _settings.GetRarityWeight(rewardData.Rarity);
    }

    public async UniTask<bool> ApplyAsync(RewardData rewardData)
    {
        if (!_currentChoices.Contains(rewardData))
            throw new InvalidOperationException("현재 선택지에 없는 보상입니다.");

        IReward reward = rewardData.RewardDefinition.CreateReward(_dependencies);

        if (!reward.CanApply())
            throw new InvalidOperationException("선택된 보상을 적용할 수 없는 상태입니다.");

        bool isApplied = await reward.ApplyAsync();

        if (!isApplied)
            throw new InvalidOperationException($"보상 적용 실패: {rewardData.RewardName}");

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
