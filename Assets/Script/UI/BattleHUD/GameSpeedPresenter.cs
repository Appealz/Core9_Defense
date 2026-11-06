using System;

public sealed class GameSpeedPresenter : IDisposable
{
    private readonly GameSpeedView _view;
    private readonly GameTimePauseService _gameTimeService;
    private readonly GameStateManager _gameStateManager;

    public GameSpeedPresenter(
        GameSpeedView view,
        GameTimePauseService gameTimeService,
        GameStateManager gameStateManager)
    {
        _view = view ?? throw new ArgumentNullException(nameof(view));
        _gameTimeService = gameTimeService ?? throw new ArgumentNullException(nameof(gameTimeService));
        _gameStateManager = gameStateManager ?? throw new ArgumentNullException(nameof(gameStateManager));

        _view.Initialize();
        _view.DecreaseClicked += HandleDecreaseClicked;
        _view.IncreaseClicked += HandleIncreaseClicked;
        _gameTimeService.SpeedChanged += HandleSpeedChanged;
        _gameStateManager.StateChanged += HandleStateChanged;
        Synchronize();
    }

    private void HandleDecreaseClicked()
    {
        _gameTimeService.TryDecreaseSpeed();
    }

    private void HandleIncreaseClicked()
    {
        _gameTimeService.TryIncreaseSpeed();
    }

    private void HandleSpeedChanged(int _)
    {
        Synchronize();
    }

    private void HandleStateChanged(GameState _, GameState __)
    {
        Synchronize();
    }

    public void Synchronize()
    {
        GameState state = _gameStateManager.CurrentState;
        bool isPlaying = state == GameState.Playing;
        bool isVisible = state == GameState.Initializing || isPlaying;
        _view.SetVisible(isVisible);
        _view.SetState(
            _gameTimeService.CurrentSpeed,
            _gameTimeService.CanDecreaseSpeed,
            _gameTimeService.CanIncreaseSpeed,
            isPlaying,
            _gameTimeService.IsTripleSpeedLocked);
    }

    public void Dispose()
    {
        _view.DecreaseClicked -= HandleDecreaseClicked;
        _view.IncreaseClicked -= HandleIncreaseClicked;
        _gameTimeService.SpeedChanged -= HandleSpeedChanged;
        _gameStateManager.StateChanged -= HandleStateChanged;
    }
}
