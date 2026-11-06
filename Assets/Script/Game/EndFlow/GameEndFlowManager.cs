using System;
using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;

public sealed class GameEndFlowManager : IDisposable
{
    public const string ContinuePlacementId = "game_over_continue";
    public const string DoubleRewardPlacementId = "game_result_double_currency";
    public const string BattleSceneName = "Battle";
    public const string MainMenuSceneName = "Lobby";

    private readonly GameStateManager _gameStateManager;
    private readonly GameManager _gameManager;
    private readonly TowerManager _towerManager;
    private readonly RunScoreCounter _scoreCounter;
    private readonly GameEndRewardPolicy _rewardPolicy;
    private readonly IPersistentCurrencyWallet _wallet;
    private readonly IRewardedAdService _rewardedAdService;
    private readonly GameCombatCleanupService _combatCleanupService;
    private readonly GameTimePauseService _timePauseService;
    private readonly float _earlyExitPenaltyRate;
    private readonly GameEndRunState _runState = new GameEndRunState();

    private GameRunSnapshot _currentSnapshot;
    private GameEndRewardSession _rewardSession;
    private GameEndRewardSession _earlyExitRewardSession;
    private bool _isBusy;

    public event Action<GameRunSnapshot, bool> GameOverReady;
    public event Action<GameRunSnapshot, int, bool> ResultReady;

    public bool IsBusy => _isBusy;

    public GameEndFlowManager(
        GameStateManager gameStateManager,
        GameManager gameManager,
        TowerManager towerManager,
        RunScoreCounter scoreCounter,
        GameEndRewardPolicy rewardPolicy,
        IPersistentCurrencyWallet wallet,
        IRewardedAdService rewardedAdService,
        GameCombatCleanupService combatCleanupService,
        GameTimePauseService timePauseService,
        float earlyExitPenaltyRate = 0.2f)
    {
        _gameStateManager = gameStateManager ?? throw new ArgumentNullException(nameof(gameStateManager));
        _gameManager = gameManager ?? throw new ArgumentNullException(nameof(gameManager));
        _towerManager = towerManager ?? throw new ArgumentNullException(nameof(towerManager));
        _scoreCounter = scoreCounter ?? throw new ArgumentNullException(nameof(scoreCounter));
        _rewardPolicy = rewardPolicy ?? throw new ArgumentNullException(nameof(rewardPolicy));
        _wallet = wallet ?? throw new ArgumentNullException(nameof(wallet));
        _rewardedAdService = rewardedAdService ?? throw new ArgumentNullException(nameof(rewardedAdService));
        _combatCleanupService = combatCleanupService ??
            throw new ArgumentNullException(nameof(combatCleanupService));
        _timePauseService = timePauseService ?? throw new ArgumentNullException(nameof(timePauseService));
        _earlyExitPenaltyRate = Math.Max(0f, Math.Min(1f, earlyExitPenaltyRate));

        _gameStateManager.StateChanged += HandleStateChanged;
    }

    private void HandleStateChanged(GameState previousState, GameState nextState)
    {
        if (nextState != GameState.GameOver)
            return;

        _currentSnapshot = CreateSnapshot();
        GameOverReady?.Invoke(
            _currentSnapshot,
            !_runState.ContinueUsed && _towerManager.CanRestoreLastDestroyedTower);
    }

    public async UniTask ContinueAsync()
    {
        if (_isBusy || _runState.ContinueUsed ||
            _gameStateManager.CurrentState != GameState.GameOver ||
            !_towerManager.CanRestoreLastDestroyedTower)
        {
            return;
        }

        _isBusy = true;

        try
        {
            bool rewardEarned = await _rewardedAdService.ShowAsync(ContinuePlacementId);

            if (!rewardEarned)
                return;

            _combatCleanupService.ClearForWaveRestart();

            if (!await _towerManager.TryRestoreLastDestroyedTowerAsync())
                return;

            _runState.TryMarkContinueUsed();
            _gameManager.ResumeCurrentWaveAfterContinue();
        }
        finally
        {
            _isBusy = false;

            if (_gameStateManager.CurrentState == GameState.GameOver)
            {
                GameOverReady?.Invoke(
                    _currentSnapshot,
                    !_runState.ContinueUsed && _towerManager.CanRestoreLastDestroyedTower);
            }
        }
    }

    public void FinishRun()
    {
        if (_isBusy || _gameStateManager.CurrentState != GameState.GameOver)
            return;

        _currentSnapshot = CreateSnapshot();
        _rewardSession = new GameEndRewardSession(_wallet, _currentSnapshot.BaseCurrencyReward);
        _rewardSession.GrantBaseReward();

        if (!_gameManager.ShowGameResult())
            return;

        ResultReady?.Invoke(_currentSnapshot, _rewardSession.TotalGranted, true);
    }

    public async UniTask DoubleRewardAsync()
    {
        if (_isBusy || _gameStateManager.CurrentState != GameState.GameResult ||
            _rewardSession == null || _rewardSession.IsAdBonusGranted)
        {
            return;
        }

        _isBusy = true;

        try
        {
            bool rewardEarned = await _rewardedAdService.ShowAsync(DoubleRewardPlacementId);

            if (rewardEarned)
                _rewardSession.GrantAdBonus();
        }
        finally
        {
            _isBusy = false;
            ResultReady?.Invoke(
                _currentSnapshot,
                _rewardSession.TotalGranted,
                !_rewardSession.IsAdBonusGranted);
        }
    }

    public void RestartGame()
    {
        if (_isBusy || _gameStateManager.CurrentState != GameState.GameResult)
            return;

        RestoreTimeAndLoad(BattleSceneName);
    }

    public void GoToMainMenu()
    {
        if (_isBusy || _gameStateManager.CurrentState != GameState.GameResult)
            return;

        RestoreTimeAndLoad(MainMenuSceneName);
    }

    public bool RestartFromPause()
    {
        return TryFinishEarlyExit(BattleSceneName);
    }

    public bool GoToMainMenuFromPause()
    {
        return TryFinishEarlyExit(MainMenuSceneName);
    }

    public EarlyExitRewardQuote GetEarlyExitRewardQuote()
    {
        return _rewardPolicy.CalculateEarlyExitReward(
            _gameManager.CurrentWaveNumber,
            _earlyExitPenaltyRate);
    }

    private bool TryFinishEarlyExit(string sceneName)
    {
        if (_isBusy || _earlyExitRewardSession != null ||
            _gameStateManager.CurrentState != GameState.Paused)
        {
            return false;
        }

        _isBusy = true;
        EarlyExitRewardQuote quote = GetEarlyExitRewardQuote();
        _earlyExitRewardSession = new GameEndRewardSession(_wallet, quote.ReceivedCoins);

        if (!_earlyExitRewardSession.GrantBaseReward())
        {
            _isBusy = false;
            return false;
        }

        RestoreTimeAndLoad(sceneName);
        return true;
    }

    private GameRunSnapshot CreateSnapshot()
    {
        int reachedWave = _gameManager.CurrentWaveNumber;
        return new GameRunSnapshot(
            _scoreCounter.Score,
            reachedWave,
            _rewardPolicy.CalculateCurrencyReward(reachedWave));
    }

    private void RestoreTimeAndLoad(string sceneName)
    {
        _timePauseService.ResetToDefault();
        SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
    }

    public void Dispose()
    {
        _gameStateManager.StateChanged -= HandleStateChanged;
    }
}
