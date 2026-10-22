
using System;

public sealed class GoldManager
{
    public event Action<int> GoldChanged;

    public int CurrentGold { get; private set; }

    public void AddGold(int amount)
    {
        if (amount <= 0)
            return;

        CurrentGold += amount;
        GoldChanged?.Invoke(CurrentGold);
    }

    public bool TrySpendGold(int amount)
    {
        if (amount <= 0 || CurrentGold < amount)
            return false;

        CurrentGold -= amount;
        GoldChanged?.Invoke(CurrentGold);

        return true;
    }

    public void ResetGold()
    {
        CurrentGold = 0;
        GoldChanged?.Invoke(CurrentGold);
    }
}
