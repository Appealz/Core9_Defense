
using System;

public class CoreRearrangementPresenter : IDisposable
{
    private readonly CoreRearrangementView _view;
    private readonly GameStateManager _gameStateManager;
    private readonly GameManager _gameManager;

    public CoreRearrangementPresenter(
        CoreRearrangementView view,
        GameStateManager gameStateManager,
        GameManager gameManager)
    {
        _view = view;
        _gameStateManager = gameStateManager;
        _gameManager = gameManager;

        _gameStateManager.StateChanged += HandleStateChanged;
        _gameManager.RearrangementTimeChanged += HandleTimeChanged;
        _view.ReadyClicked += HandleReadyClicked;

        if (_gameStateManager.CurrentState == GameState.Rearrangement)
            _view.Show(_gameManager.RearrangementRemainingSeconds);
        else
            _view.Hide();
    }

    private void HandleStateChanged(GameState previousState, GameState nextState)
    {
        if (nextState == GameState.Rearrangement)
            _view.Show(_gameManager.RearrangementRemainingSeconds);
        else
            _view.Hide();
    }

    private void HandleTimeChanged(int remainingSeconds)
    {
        _view.SetRemainingSeconds(remainingSeconds);
    }

    private void HandleReadyClicked()
    {
        _gameManager.StartNextWave();
    }

    public void Dispose()
    {
        _gameStateManager.StateChanged -= HandleStateChanged;
        _gameManager.RearrangementTimeChanged -= HandleTimeChanged;
        _view.ReadyClicked -= HandleReadyClicked;
    }
}
