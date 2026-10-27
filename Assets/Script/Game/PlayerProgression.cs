using System;

public sealed class PlayerProgression
{
    private readonly PlayerLevelData _levelData;

    public int Level { get; private set; } = 1;
    public int CurrentExperience { get; private set; }
    public int PendingRewardCount { get; private set; }
    public int RequiredExperience => _levelData.GetRequiredExperience(Level);

    public event Action<int, int, int> ProgressChanged;
    public event Action<int> RewardCountChanged;

    public PlayerProgression(PlayerLevelData levelData)
    {
        _levelData = levelData ?? throw new ArgumentNullException(nameof(levelData));
    }

    public void Reset()
    {
        Level = 1;
        CurrentExperience = 0;
        PendingRewardCount = 0;
        ProgressChanged?.Invoke(Level, CurrentExperience, RequiredExperience);
        RewardCountChanged?.Invoke(PendingRewardCount);
    }

    public void AddExperience(int amount)
    {
        if (amount <= 0)
            return;

        CurrentExperience = (int)Math.Min(int.MaxValue, (long)CurrentExperience + amount);
        int levelUpCount = 0;

        while (CurrentExperience >= RequiredExperience)
        {
            CurrentExperience -= RequiredExperience;
            Level++;
            levelUpCount++;
        }

        if (levelUpCount > 0)
        {
            PendingRewardCount = (int)Math.Min(int.MaxValue, (long)PendingRewardCount + levelUpCount);
            RewardCountChanged?.Invoke(PendingRewardCount);
        }

        ProgressChanged?.Invoke(Level, CurrentExperience, RequiredExperience);
    }

    public bool TryConsumeReward()
    {
        if (PendingRewardCount <= 0)
            return false;

        PendingRewardCount--;
        RewardCountChanged?.Invoke(PendingRewardCount);
        return true;
    }
}
