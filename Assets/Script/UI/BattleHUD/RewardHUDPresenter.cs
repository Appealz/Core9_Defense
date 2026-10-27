using System;

public sealed class RewardHUDPresenter : IDisposable
{
    private readonly RewardHUDView _view;
    private readonly PlayerProgression _playerProgression;

    public RewardHUDPresenter(RewardHUDView view, PlayerProgression playerProgression)
    {
        _view = view;
        _playerProgression = playerProgression;
        _playerProgression.RewardCountChanged += HandleRewardCountChanged;
        _view.SetRewardCount(_playerProgression.PendingRewardCount);
    }

    private void HandleRewardCountChanged(int amount)
    {
        _view.SetRewardCount(amount);
    }

    public void Dispose()
    {
        _playerProgression.RewardCountChanged -= HandleRewardCountChanged;
    }
}
