using System;

public class WaveHUDPresenter : IDisposable
{
    private readonly WaveHUDView _view;
    private readonly GameManager _gameManager;
    private readonly GameStateManager _gameStateManager;
    private readonly PlayerProgression _playerProgression;

    public WaveHUDPresenter(
        WaveHUDView view,
        GameManager gameManager,
        GameStateManager gameStateManager,
        PlayerProgression playerProgression)
    {
        _view = view;
        _gameManager = gameManager;
        _gameStateManager = gameStateManager;
        _playerProgression = playerProgression;

        _gameStateManager.StateChanged += HandleStateChanged;
        _playerProgression.ProgressChanged += HandleProgressChanged;

        _view.SetWaveNumber(_gameManager.CurrentWaveNumber);
        _view.SetLevel(_playerProgression.Level);
        _view.SetExperience(_playerProgression.CurrentExperience, _playerProgression.RequiredExperience);
    }

    private void HandleProgressChanged(int level, int currentExperience, int requiredExperience)
    {
        _view.SetLevel(level);
        _view.SetExperience(currentExperience, requiredExperience);
    }

    private void HandleStateChanged(GameState previousState, GameState nextState)
    {
        if (nextState == GameState.Playing)
            _view.SetWaveNumber(_gameManager.CurrentWaveNumber);
    }

    public void Dispose()
    {
        _gameStateManager.StateChanged -= HandleStateChanged;
        _playerProgression.ProgressChanged -= HandleProgressChanged;
    }
}
