using System;

public sealed class BattleSettingsPresenter : IDisposable
{
    private readonly BattleSettingsView _view;
    private readonly BattleSettingsInput _input;
    private readonly GameStateManager _gameStateManager;
    private readonly GameEndFlowManager _gameEndFlowManager;

    public BattleSettingsPresenter(
        BattleSettingsView view,
        BattleSettingsInput input,
        GameStateManager gameStateManager,
        GameEndFlowManager gameEndFlowManager)
    {
        _view = view ?? throw new ArgumentNullException(nameof(view));
        _input = input ?? throw new ArgumentNullException(nameof(input));
        _gameStateManager = gameStateManager ?? throw new ArgumentNullException(nameof(gameStateManager));
        _gameEndFlowManager = gameEndFlowManager ?? throw new ArgumentNullException(nameof(gameEndFlowManager));

        _view.Initialize();
        _input.Initialize();
        _view.SettingsClicked += Toggle;
        _view.ResumeClicked += Resume;
        _view.ReturnToLobbyClicked += ReturnToLobby;
        _view.RestartClicked += Restart;
        _input.ToggleRequested += Toggle;
        _gameStateManager.StateChanged += HandleStateChanged;

        _view.SetOpen(false);
        _view.SetSettingsButtonInteractable(
            _gameStateManager.CurrentState == GameState.Playing);
    }

    private void Toggle()
    {
        if (_view.IsOpen)
        {
            Resume();
            return;
        }

        if (_gameStateManager.CurrentState != GameState.Playing)
            return;

        _gameStateManager.ChangeState(GameState.Paused);
        _view.SetOpen(true);
    }

    private void Resume()
    {
        if (!_view.IsOpen || _gameStateManager.CurrentState != GameState.Paused)
            return;

        _view.SetOpen(false);
        _gameStateManager.ChangeState(GameState.Playing);
    }

    private void ReturnToLobby()
    {
        if (!_view.IsOpen || _gameStateManager.CurrentState != GameState.Paused)
            return;

        _view.SetOpen(false);

        if (!_gameEndFlowManager.GoToMainMenuFromPause())
            _view.SetOpen(true);
    }

    private void Restart()
    {
        if (!_view.IsOpen || _gameStateManager.CurrentState != GameState.Paused)
            return;

        _view.SetOpen(false);

        if (!_gameEndFlowManager.RestartFromPause())
            _view.SetOpen(true);
    }

    private void HandleStateChanged(GameState _, GameState nextState)
    {
        _view.SetSettingsButtonInteractable(nextState == GameState.Playing);

        if (nextState != GameState.Paused && _view.IsOpen)
            _view.SetOpen(false);
    }

    public void Dispose()
    {
        if (_view != null)
        {
            _view.SettingsClicked -= Toggle;
            _view.ResumeClicked -= Resume;
            _view.ReturnToLobbyClicked -= ReturnToLobby;
            _view.RestartClicked -= Restart;
        }

        if (_input != null)
            _input.ToggleRequested -= Toggle;

        _gameStateManager.StateChanged -= HandleStateChanged;
    }
}
