using Cysharp.Threading.Tasks;

public sealed class GoldReward : IReward
{
    private readonly GoldManager _goldManager;
    private readonly int _amount;

    public GoldReward(GoldManager goldManager, int amount)
    {
        _goldManager = goldManager;
        _amount = amount;
    }

    public bool CanApply()
    {
        return _goldManager != null && _amount > 0;
    }

    public UniTask<bool> ApplyAsync()
    {
        if (!CanApply())
            return UniTask.FromResult(false);

        _goldManager.AddGold(_amount);
        return UniTask.FromResult(true);
    }
}
