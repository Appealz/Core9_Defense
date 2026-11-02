using System;

public sealed class GameEndRewardPolicy
{
    public int ScorePerEnemy { get; }
    public int BaseCurrency { get; }
    public int CurrencyPerWave { get; }

    public GameEndRewardPolicy(
        int scorePerEnemy = 100,
        int baseCurrency = 30,
        int currencyPerWave = 10)
    {
        if (scorePerEnemy < 0)
            throw new ArgumentOutOfRangeException(nameof(scorePerEnemy));

        if (baseCurrency < 0)
            throw new ArgumentOutOfRangeException(nameof(baseCurrency));

        if (currencyPerWave < 0)
            throw new ArgumentOutOfRangeException(nameof(currencyPerWave));

        ScorePerEnemy = scorePerEnemy;
        BaseCurrency = baseCurrency;
        CurrencyPerWave = currencyPerWave;
    }

    public int CalculateScore(int defeatedEnemyCount)
    {
        if (defeatedEnemyCount <= 0)
            return 0;

        return (int)Math.Min(int.MaxValue, (long)defeatedEnemyCount * ScorePerEnemy);
    }

    public int CalculateCurrencyReward(int reachedWave)
    {
        int safeWave = Math.Max(1, reachedWave);
        return (int)Math.Min(int.MaxValue, (long)BaseCurrency + (long)safeWave * CurrencyPerWave);
    }
}
