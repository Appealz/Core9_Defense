using System;
using UnityEngine;

public interface IGemsWallet
{
    int Balance { get; }
    event Action<int> BalanceChanged;
    bool Add(int amount);
}

public sealed class GemsWallet : IGemsWallet
{
    public const string DefaultPlayerPrefsKey = "Core9.Gems";

    private readonly string _playerPrefsKey;
    private int _balance;

    public int Balance => _balance;
    public event Action<int> BalanceChanged;

    public GemsWallet(string playerPrefsKey = DefaultPlayerPrefsKey)
    {
        _playerPrefsKey = string.IsNullOrWhiteSpace(playerPrefsKey)
            ? throw new ArgumentException("A PlayerPrefs key is required.", nameof(playerPrefsKey))
            : playerPrefsKey;
        _balance = Math.Max(0, PlayerPrefs.GetInt(_playerPrefsKey, 0));
    }

    public bool Add(int amount)
    {
        if (amount <= 0)
            return false;

        _balance = (int)Math.Min(int.MaxValue, (long)_balance + amount);
        PlayerPrefs.SetInt(_playerPrefsKey, _balance);
        PlayerPrefs.Save();
        BalanceChanged?.Invoke(_balance);
        return true;
    }
}
