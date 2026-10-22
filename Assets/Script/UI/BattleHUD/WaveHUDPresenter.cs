
using System;

public class WaveHUDPresenter : IDisposable
{
    private readonly WaveHUDView _view;
    private readonly WaveManager _waveManager;
    private readonly GameManager _gameManager;
    private readonly GameStateManager _gameStateManager;

    public WaveHUDPresenter(WaveHUDView view, WaveManager waveManager, GameManager gameManager, GameStateManager gameStateManager)
    {
        _view = view;
        _waveManager = waveManager;
        _gameManager = gameManager;
        _gameStateManager = gameStateManager;

        _waveManager.ProgressChanged += HandleProgressChanged;
        _gameStateManager.StateChanged += HandleStateChanged;

        _view.SetWaveNumber(_gameManager.CurrentWaveNumber);
        _view.SetProgress(0f);
    }

    private void HandleProgressChanged(int processedCount, int totalCount)
    {
        _view.SetProgress(processedCount, totalCount);
    }

    private void HandleStateChanged(GameState previousState, GameState nextState)
    {
        if (nextState != GameState.Playing)
            return;

        _view.SetWaveNumber(_gameManager.CurrentWaveNumber);
    }

    public void Dispose()
    {
        _waveManager.ProgressChanged -= HandleProgressChanged;
        _gameStateManager.StateChanged -= HandleStateChanged;
    }
}
