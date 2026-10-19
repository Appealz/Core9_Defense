
using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

public class RewardSelectionManager
{
    private readonly IReadOnlyList<RewardData> _rewardDataList;
    private readonly RewardDependencies _dependencies;
    private readonly Random _random = new();

    private readonly List<RewardData> _currentChoices = new();

    public IReadOnlyList<RewardData> CurrentChoices => _currentChoices;

    public RewardSelectionManager(IReadOnlyList<RewardData> rewardDataList, RewardDependencies dependencies)
    {
        _rewardDataList = rewardDataList;
        _dependencies = dependencies;
    }

    public IReadOnlyList<RewardData> CreateChoices(int count)
    {
        List<RewardData> candidates = new();

        foreach (RewardData rewardData in _rewardDataList)
        {
            IReward reward = rewardData.RewardDefinition.CreateReward(_dependencies);

            if (reward.CanApply())
                candidates.Add(rewardData);
        }

        Shuffle(candidates);

        _currentChoices.Clear();

        int choiceCount = Math.Min(count, candidates.Count);

        for (int i = 0; i < choiceCount; i++)
            _currentChoices.Add(candidates[i]);

        return _currentChoices;
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
