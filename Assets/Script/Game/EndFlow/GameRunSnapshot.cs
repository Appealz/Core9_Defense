public readonly struct GameRunSnapshot
{
    public int Score { get; }
    public int ReachedWave { get; }
    public int BaseCurrencyReward { get; }

    public GameRunSnapshot(int score, int reachedWave, int baseCurrencyReward)
    {
        Score = score < 0 ? 0 : score;
        ReachedWave = reachedWave < 1 ? 1 : reachedWave;
        BaseCurrencyReward = baseCurrencyReward < 0 ? 0 : baseCurrencyReward;
    }
}
