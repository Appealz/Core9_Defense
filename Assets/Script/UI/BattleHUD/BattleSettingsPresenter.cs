using System;

public sealed class BattleSettingsPresenter : IDisposable
{
    private enum PendingExitAction
    {
        None,
        ReturnToLobby,
        Restart
    }

    private readonly BattleSettingsView _view;
    private readonly BattleExitConfirmationView _exitConfirmationView;
    private readonly BattleSettingsInput _input;
    private readonly GameStateManager _gameStateManager;
    private readonly GameEndFlowManager _gameEndFlowManager;
    private readonly UserAudioSettings _audioSettings;
    private PendingExitAction _pendingExitAction;

    public BattleSettingsPresenter(
        BattleSettingsView view,
        BattleExitConfirmationView exitConfirmationView,
        BattleSettingsInput input,
        GameStateManager gameStateManager,
        GameEndFlowManager gameEndFlowManager,
        UserAudioSettings audioSettings = null)
    {
        _view = view ?? throw new ArgumentNullException(nameof(view));
        _exitConfirmationView = exitConfirmationView ??
            throw new ArgumentNullException(nameof(exitConfirmationView));
        _input = input ?? throw new ArgumentNullException(nameof(input));
        _gameStateManager = gameStateManager ?? throw new ArgumentNullException(nameof(gameStateManager));
        _gameEndFlowManager = gameEndFlowManager ?? throw new ArgumentNullException(nameof(gameEndFlowManager));
        _audioSettings = audioSettings ?? new UserAudioSettings();

        _view.Initialize();
        _exitConfirmationView.Initialize();
        _input.Initialize();
        _view.SettingsClicked += Toggle;
        _view.ResumeClicked += Resume;
        _view.ReturnToLobbyClicked += ReturnToLobby;
        _view.RestartClicked += Restart;
        _view.BgmVolumeChanged += _audioSettings.SetBgmVolume;
        _view.SfxVolumeChanged += _audioSettings.SetSfxVolume;
        _exitConfirmationView.CancelClicked += CloseExitConfirmation;
        _exitConfirmationView.ConfirmClicked += ConfirmEarlyExit;
        _input.ToggleRequested += Toggle;
        _gameStateManager.StateChanged += HandleStateChanged;

        _view.SetOpen(false);
        _exitConfirmationView.Hide();
        _view.SetAudioVolumes(_audioSettings.BgmVolume, _audioSettings.SfxVolume);
        _view.SetSettingsButtonInteractable(
            _gameStateManager.CurrentState == GameState.Playing);
    }

    private void Toggle()
    {
        if (_exitConfirmationView.IsVisible)
        {
            CloseExitConfirmation();
            return;
        }

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
        if (_exitConfirmationView.IsVisible || !_view.IsOpen ||
            _gameStateManager.CurrentState != GameState.Paused)
            return;

        _view.SetOpen(false);
        _gameStateManager.ChangeState(GameState.Playing);
    }

    private void ReturnToLobby()
    {
        ShowExitConfirmation(
            PendingExitAction.ReturnToLobby,
            "RETURN TO LOBBY?",
            "Are you sure you want to return to the lobby?",
            "RETURN");
    }

    private void Restart()
    {
        ShowExitConfirmation(
            PendingExitAction.Restart,
            "RESTART GAME?",
            "Are you sure you want to restart the game?",
            "RESTART");
    }

    private void ShowExitConfirmation(
        PendingExitAction action,
        string title,
        string description,
        string confirmButtonLabel)
    {
        if (_exitConfirmationView.IsVisible || !_view.IsOpen ||
            _gameStateManager.CurrentState != GameState.Paused ||
            _gameEndFlowManager.IsBusy)
        {
            return;
        }

        _pendingExitAction = action;
        _exitConfirmationView.Show(
            title,
            description,
            confirmButtonLabel,
            _gameEndFlowManager.GetEarlyExitRewardQuote());
    }

    private void CloseExitConfirmation()
    {
        if (_gameEndFlowManager.IsBusy)
            return;

        _pendingExitAction = PendingExitAction.None;
        _exitConfirmationView.Hide();
    }

    private void ConfirmEarlyExit()
    {
        if (!_exitConfirmationView.IsVisible || _gameEndFlowManager.IsBusy)
            return;

        _exitConfirmationView.SetButtonsInteractable(false);

        bool started = _pendingExitAction switch
        {
            PendingExitAction.ReturnToLobby => _gameEndFlowManager.GoToMainMenuFromPause(),
            PendingExitAction.Restart => _gameEndFlowManager.RestartFromPause(),
            _ => false
        };

        if (!started)
            _exitConfirmationView.SetButtonsInteractable(true);
    }

    private void HandleStateChanged(GameState _, GameState nextState)
    {
        _view.SetSettingsButtonInteractable(nextState == GameState.Playing);

        if (nextState != GameState.Paused && _view.IsOpen)
            _view.SetOpen(false);

        if (nextState != GameState.Paused)
        {
            _pendingExitAction = PendingExitAction.None;
            _exitConfirmationView.Hide();
        }
    }

    public void Dispose()
    {
        if (_view != null)
        {
            _view.SettingsClicked -= Toggle;
            _view.ResumeClicked -= Resume;
            _view.ReturnToLobbyClicked -= ReturnToLobby;
            _view.RestartClicked -= Restart;
            _view.BgmVolumeChanged -= _audioSettings.SetBgmVolume;
            _view.SfxVolumeChanged -= _audioSettings.SetSfxVolume;
        }

        if (_exitConfirmationView != null)
        {
            _exitConfirmationView.CancelClicked -= CloseExitConfirmation;
            _exitConfirmationView.ConfirmClicked -= ConfirmEarlyExit;
        }

        if (_input != null)
            _input.ToggleRequested -= Toggle;

        _gameStateManager.StateChanged -= HandleStateChanged;
    }
}
