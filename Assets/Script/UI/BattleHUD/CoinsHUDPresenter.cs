using System;

public sealed class CoinsHUDPresenter : IDisposable
{
    private readonly CoinsHUDView _view;
    private readonly GameManager _gameManager;
    private readonly GameEndRewardPolicy _rewardPolicy;

    public CoinsHUDPresenter(
        CoinsHUDView view,
        GameManager gameManager,
        GameEndRewardPolicy rewardPolicy)
    {
        _view = view ?? throw new ArgumentNullException(nameof(view));
        _gameManager = gameManager ?? throw new ArgumentNullException(nameof(gameManager));
        _rewardPolicy = rewardPolicy ?? throw new ArgumentNullException(nameof(rewardPolicy));

        _gameManager.WaveNumberChanged += HandleWaveNumberChanged;
        Refresh(_gameManager.CurrentWaveNumber);
    }

    private void HandleWaveNumberChanged(int waveNumber)
    {
        Refresh(waveNumber);
    }

    private void Refresh(int waveNumber)
    {
        _view.SetCoins(_rewardPolicy.CalculateCurrencyReward(waveNumber));
    }

    public void Dispose()
    {
        _gameManager.WaveNumberChanged -= HandleWaveNumberChanged;
    }
}
