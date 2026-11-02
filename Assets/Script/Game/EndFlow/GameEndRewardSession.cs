using System;

public sealed class GameEndRewardSession
{
    private readonly IPersistentCurrencyWallet _wallet;

    public int BaseReward { get; }
    public int TotalGranted { get; private set; }
    public bool IsBaseRewardGranted { get; private set; }
    public bool IsAdBonusGranted { get; private set; }

    public GameEndRewardSession(IPersistentCurrencyWallet wallet, int baseReward)
    {
        _wallet = wallet ?? throw new ArgumentNullException(nameof(wallet));
        BaseReward = Math.Max(0, baseReward);
    }

    public bool GrantBaseReward()
    {
        if (IsBaseRewardGranted)
            return false;

        IsBaseRewardGranted = true;

        if (BaseReward > 0)
        {
            _wallet.Add(BaseReward);
            TotalGranted = BaseReward;
        }

        return true;
    }

    public bool GrantAdBonus()
    {
        if (!IsBaseRewardGranted || IsAdBonusGranted)
            return false;

        IsAdBonusGranted = true;

        if (BaseReward > 0)
        {
            _wallet.Add(BaseReward);
            TotalGranted = (int)Math.Min(int.MaxValue, (long)TotalGranted + BaseReward);
        }

        return true;
    }
}
