
using System;

public sealed class GoldHUDPresenter : IDisposable
{
    private readonly GoldHUDView _view;
    private readonly GoldManager _goldManager;

    public GoldHUDPresenter(GoldHUDView view, GoldManager goldManager)
    {
        _view = view;
        _goldManager = goldManager;

        _goldManager.GoldChanged += HandleGoldChanged;
        _goldManager.GoldGained += HandleGoldGained;
        _goldManager.GoldSpent += HandleGoldSpent;

        _view.SetGold(_goldManager.CurrentGold);
    }

    private void HandleGoldChanged(int amount)
    {
        _view.SetGold(amount);
    }

    private void HandleGoldGained(int amount)
    {
        _view.PlayGoldGain(amount);
    }

    private void HandleGoldSpent(int amount)
    {
        _view.PlayGoldSpend(amount);
    }

    public void Dispose()
    {
        _goldManager.GoldChanged -= HandleGoldChanged;
        _goldManager.GoldGained -= HandleGoldGained;
        _goldManager.GoldSpent -= HandleGoldSpent;
    }
}
