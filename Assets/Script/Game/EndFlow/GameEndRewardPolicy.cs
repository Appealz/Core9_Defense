using System;

public readonly struct EarlyExitRewardQuote
{
    public int EarnedCoins { get; }
    public int PenaltyCoins { get; }
    public int ReceivedCoins { get; }
    public int PenaltyPercent { get; }

    public EarlyExitRewardQuote(
        int earnedCoins,
        int penaltyCoins,
        int receivedCoins,
        int penaltyPercent)
    {
        EarnedCoins = Math.Max(0, earnedCoins);
        PenaltyCoins = Math.Max(0, penaltyCoins);
        ReceivedCoins = Math.Max(0, receivedCoins);
        PenaltyPercent = Math.Max(0, penaltyPercent);
    }
}

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

    public EarlyExitRewardQuote CalculateEarlyExitReward(int reachedWave, float penaltyRate)
    {
        decimal safePenaltyRate = (decimal)Math.Max(0f, Math.Min(1f, penaltyRate));
        int earnedCoins = CalculateCurrencyReward(reachedWave);
        int receivedCoins = (int)decimal.Floor(earnedCoins * (1m - safePenaltyRate));
        int penaltyCoins = earnedCoins - receivedCoins;
        int penaltyPercent = (int)decimal.Round(
            safePenaltyRate * 100m,
            0,
            MidpointRounding.AwayFromZero);

        return new EarlyExitRewardQuote(
            earnedCoins,
            penaltyCoins,
            receivedCoins,
            penaltyPercent);
    }
}
