using System;
using UnityEngine;

public interface IPersistentCurrencyWallet
{
    int Balance { get; }
    event Action<int> BalanceChanged;
    bool Add(int amount);
}

public sealed class PersistentCurrencyWallet : IPersistentCurrencyWallet, IPersistentCurrencySpender
{
    public const string DefaultPlayerPrefsKey = "Core9.PersistentCurrency";

    private readonly string _playerPrefsKey;

    public int Balance => Math.Max(0, PlayerPrefs.GetInt(_playerPrefsKey, 0));
    public event Action<int> BalanceChanged;

    public PersistentCurrencyWallet(string playerPrefsKey = DefaultPlayerPrefsKey)
    {
        _playerPrefsKey = string.IsNullOrWhiteSpace(playerPrefsKey)
            ? throw new ArgumentException("A PlayerPrefs key is required.", nameof(playerPrefsKey))
            : playerPrefsKey;
    }

    public bool Add(int amount)
    {
        if (amount <= 0)
            return false;

        int nextBalance = (int)Math.Min(int.MaxValue, (long)Balance + amount);
        PlayerPrefs.SetInt(_playerPrefsKey, nextBalance);
        PlayerPrefs.Save();
        BalanceChanged?.Invoke(nextBalance);
        return true;
    }

    public bool TrySpend(int amount)
    {
        if (amount <= 0 || Balance < amount)
            return false;

        int nextBalance = Balance - amount;
        PlayerPrefs.SetInt(_playerPrefsKey, nextBalance);
        PlayerPrefs.Save();
        BalanceChanged?.Invoke(nextBalance);
        return true;
    }

    public void Reset()
    {
        PlayerPrefs.DeleteKey(_playerPrefsKey);
        PlayerPrefs.Save();
        BalanceChanged?.Invoke(0);
    }
}
