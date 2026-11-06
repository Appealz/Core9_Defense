using System;
using NUnit.Framework;

public sealed class GameEndFlowTests
{
    [Test]
    public void RewardPolicy_UsesConfigurableEnemyAndWaveRules()
    {
        var policy = new GameEndRewardPolicy(
            scorePerEnemy: 125,
            baseCurrency: 25,
            currencyPerWave: 12);

        Assert.AreEqual(1_500, policy.CalculateScore(12));
        Assert.AreEqual(169, policy.CalculateCurrencyReward(12));
    }

    [Test]
    public void RewardPolicy_EarlyExitAppliesPenaltyOnlyToCurrentRunReward()
    {
        var policy = new GameEndRewardPolicy(
            baseCurrency: 30,
            currencyPerWave: 10);

        EarlyExitRewardQuote quote = policy.CalculateEarlyExitReward(7, 0.2f);

        Assert.AreEqual(100, quote.EarnedCoins);
        Assert.AreEqual(20, quote.PenaltyCoins);
        Assert.AreEqual(80, quote.ReceivedCoins);
        Assert.AreEqual(20, quote.PenaltyPercent);
    }

    [Test]
    public void RewardPolicy_EarlyExitFloorsReceivedCoins()
    {
        var policy = new GameEndRewardPolicy(
            baseCurrency: 91,
            currencyPerWave: 10);

        EarlyExitRewardQuote quote = policy.CalculateEarlyExitReward(1, 0.2f);

        Assert.AreEqual(101, quote.EarnedCoins);
        Assert.AreEqual(21, quote.PenaltyCoins);
        Assert.AreEqual(80, quote.ReceivedCoins);
    }

    [Test]
    public void RewardSession_GrantsBaseAndAdBonusOnlyOnce()
    {
        var wallet = new MemoryWallet();
        var session = new GameEndRewardSession(wallet, 150);

        Assert.IsTrue(session.GrantBaseReward());
        Assert.IsFalse(session.GrantBaseReward());
        Assert.AreEqual(150, wallet.Balance);

        Assert.IsTrue(session.GrantAdBonus());
        Assert.IsFalse(session.GrantAdBonus());
        Assert.AreEqual(300, wallet.Balance);
        Assert.AreEqual(300, session.TotalGranted);
    }

    [Test]
    public void RewardSession_DoesNotGrantAdBonusBeforeBaseReward()
    {
        var wallet = new MemoryWallet();
        var session = new GameEndRewardSession(wallet, 150);

        Assert.IsFalse(session.GrantAdBonus());
        Assert.AreEqual(0, wallet.Balance);
    }

    [Test]
    public void Snapshot_ClampsInvalidDisplayValues()
    {
        var snapshot = new GameRunSnapshot(-1, 0, -5);

        Assert.AreEqual(0, snapshot.Score);
        Assert.AreEqual(1, snapshot.ReachedWave);
        Assert.AreEqual(0, snapshot.BaseCurrencyReward);
    }

    [Test]
    public void RunState_AllowsContinueOnlyOnce()
    {
        var state = new GameEndRunState();

        Assert.IsTrue(state.TryMarkContinueUsed());
        Assert.IsFalse(state.TryMarkContinueUsed());
        Assert.IsTrue(state.ContinueUsed);
    }

    private sealed class MemoryWallet : IPersistentCurrencyWallet
    {
        public int Balance { get; private set; }
        public event Action<int> BalanceChanged;

        public bool Add(int amount)
        {
            if (amount <= 0)
                return false;

            Balance += amount;
            BalanceChanged?.Invoke(Balance);
            return true;
        }
    }
}
