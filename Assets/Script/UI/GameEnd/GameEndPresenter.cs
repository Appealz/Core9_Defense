using System;
using Cysharp.Threading.Tasks;

public sealed class GameEndPresenter : IDisposable
{
    private readonly GameEndView _view;
    private readonly GameEndFlowManager _flowManager;
    private readonly GameStateManager _gameStateManager;

    public GameEndPresenter(
        GameEndView view,
        GameEndFlowManager flowManager,
        GameStateManager gameStateManager)
    {
        _view = view ?? throw new ArgumentNullException(nameof(view));
        _flowManager = flowManager ?? throw new ArgumentNullException(nameof(flowManager));
        _gameStateManager = gameStateManager ?? throw new ArgumentNullException(nameof(gameStateManager));

        _view.Initialize();
        _view.ContinueClicked += HandleContinueClicked;
        _view.FinishClicked += HandleFinishClicked;
        _view.DoubleRewardClicked += HandleDoubleRewardClicked;
        _view.RestartClicked += HandleRestartClicked;
        _view.MainMenuClicked += HandleMainMenuClicked;
        _flowManager.GameOverReady += HandleGameOverReady;
        _flowManager.ResultReady += HandleResultReady;
        _gameStateManager.StateChanged += HandleStateChanged;
    }

    private void HandleGameOverReady(GameRunSnapshot snapshot, bool canContinue)
    {
        _view.ShowGameOver(snapshot, canContinue);
    }

    private void HandleResultReady(GameRunSnapshot snapshot, int totalReward, bool canDoubleReward)
    {
        _view.ShowResult(snapshot, totalReward, canDoubleReward);
    }

    private void HandleStateChanged(GameState previousState, GameState nextState)
    {
        if (nextState == GameState.Playing)
            _view.Hide();
    }

    private void HandleContinueClicked()
    {
        _view.SetBusy(true);
        _flowManager.ContinueAsync().Forget();
    }

    private void HandleFinishClicked()
    {
        _view.SetBusy(true);
        _flowManager.FinishRun();
    }

    private void HandleDoubleRewardClicked()
    {
        _view.SetBusy(true);
        _flowManager.DoubleRewardAsync().Forget();
    }

    private void HandleRestartClicked()
    {
        _view.SetBusy(true);
        _flowManager.RestartGame();
    }

    private void HandleMainMenuClicked()
    {
        _view.SetBusy(true);
        _flowManager.GoToMainMenu();
    }

    public void Dispose()
    {
        _view.ContinueClicked -= HandleContinueClicked;
        _view.FinishClicked -= HandleFinishClicked;
        _view.DoubleRewardClicked -= HandleDoubleRewardClicked;
        _view.RestartClicked -= HandleRestartClicked;
        _view.MainMenuClicked -= HandleMainMenuClicked;
        _flowManager.GameOverReady -= HandleGameOverReady;
        _flowManager.ResultReady -= HandleResultReady;
        _gameStateManager.StateChanged -= HandleStateChanged;
    }
}
