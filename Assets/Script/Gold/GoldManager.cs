
using System;

public sealed class GoldManager
{
    public event Action<int> GoldChanged;
    public event Action<int> GoldGained;
    public event Action<int> GoldSpent;

    public int CurrentGold { get; private set; }

    public void AddGold(int amount)
    {
        if (amount <= 0)
            return;

        int previousGold = CurrentGold;
        CurrentGold = (int)Math.Min(int.MaxValue, (long)CurrentGold + amount);
        GoldChanged?.Invoke(CurrentGold);

        int actualGain = CurrentGold - previousGold;

        if (actualGain > 0)
            GoldGained?.Invoke(actualGain);
    }

    public bool TrySpendGold(int amount)
    {
        if (amount <= 0 || CurrentGold < amount)
            return false;

        CurrentGold -= amount;
        GoldChanged?.Invoke(CurrentGold);
        GoldSpent?.Invoke(amount);

        return true;
    }

    public void ResetGold()
    {
        CurrentGold = 0;
        GoldChanged?.Invoke(CurrentGold);
    }
}
