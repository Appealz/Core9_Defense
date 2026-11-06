
using Cysharp.Threading.Tasks;
using UnityEngine;

public sealed class GameBootstrapper : SceneBootstrapper
{
    [SerializeField] private GameStartData _gameStartData;
    [SerializeField] private CoreLayout _coreLayout;
    [SerializeField] private Camera _mainCamera;
    [SerializeField] private Transform _objectPoolsRoot;
    [SerializeField] private RectTransform _combatTextRoot;
    [SerializeField] private BattleSettingsView _settingsView;
    [SerializeField] private BattleExitConfirmationView _exitConfirmationView;
    [SerializeField] private BattleSettingsInput _settingsInput;
    [SerializeField] private RewardSelectionView _rewardSelectionView;
    [SerializeField] private CoreRearrangementView _rearrangementView;
    [SerializeField] private WaveHUDView _waveHUDView;
    [SerializeField] private GoldHUDView _goldHUDView;
    [SerializeField] private CoinsHUDView _coinsHUDView;
    [SerializeField] private ScoreHUDView _scoreHUDView;
    [SerializeField] private RewardHUDView _rewardHUDView;
    [SerializeField] private CoreGoldUpgradeView _coreGoldUpgradeView;
    [SerializeField] private TowerGoldUpgradeView _towerGoldUpgradeView;
    [SerializeField] private GameSpeedView _gameSpeedView;
    [SerializeField] private PreparationAutoSkipView _preparationAutoSkipView;
    [Header("Flying Gems")]
    [SerializeField] private FlyingGem _flyingGemPrefab;
    [SerializeField] private Transform _flyingGemPoolRoot;
    [SerializeField] private FlyingGemSettings _flyingGemSettings = new();
    [SerializeField] private float _enemySpawnMargin = 1f;
    [SerializeField, Min(0f)] private float _battleStartDelay = 1f;
    [Header("Early Exit Reward")]
    [SerializeField, Range(0f, 1f)] private float _earlyExitCoinsPenaltyRate = 0.2f;

    private RewardSelectionPresenter _rewardSelectionPresenter;
    private CoreRearrangementPresenter _rearrangementPresenter;
    private WaveHUDPresenter _waveHUDPresenter;
    private GoldHUDPresenter _goldHUDPresenter;
    private CoinsHUDPresenter _coinsHUDPresenter;
    private ScoreHUDPresenter _scoreHUDPresenter;
    private RewardHUDPresenter _rewardHUDPresenter;
    private CoreGoldUpgradePresenter _coreGoldUpgradePresenter;
    private TowerGoldUpgradePresenter _towerGoldUpgradePresenter;
    private TowerGoldUpgradeManager _towerGoldUpgradeManager;
    private TowerSelectionManager _towerSelectionManager;
    private TowerManager _towerManager;
    private GameManager _gameManager;
    private GameEndPresenter _gameEndPresenter;
    private GameEndFlowManager _gameEndFlowManager;
    private GameTimePauseService _gameTimePauseService;
    private GameSpeedPresenter _gameSpeedPresenter;
    private PreparationAutoSkipPresenter _preparationAutoSkipPresenter;
    private BattleSettingsPresenter _settingsPresenter;
    private RunScoreCounter _runScoreCounter;
    private FlyingGemService _flyingGemService;

    private GoldManager _goldManager;
    private EnemyGoldRewardService _enemyGoldRewardService;
    private EnemyExperienceRewardService _enemyExperienceRewardService;
    private EnemyDeathBehaviorService _deathBehaviorService;

    protected override async UniTask ComposeAsync()
    {
        Time.timeScale = 1f;

        if (_gameStartData == null)
        {
            Debug.LogError("GameStartData is missing.", this);
            return;
        }

        if (_gameStartData.PersistentUpgradeCatalog == null)
        {
            Debug.LogError("PersistentUpgradeCatalog is missing from GameStartData.", this);
            return;
        }

        CoreBoundary coreBoundary = _coreLayout.GetComponentInChildren<CoreBoundary>();
        CoreVisual coreVisual = _coreLayout.GetComponentInChildren<CoreVisual>();
        TowerSpawnTester towerSpawnTester = GetComponent<TowerSpawnTester>();

        if (coreBoundary == null || coreVisual == null)
        {
            Debug.LogError("CoreBoundary 또는 CoreVisual을 찾을 수 없습니다.", this);
            return;
        }

        if (towerSpawnTester == null)
        {
            Debug.LogError("TowerSpawnTester를 찾을 수 없습니다.", this);
            return;
        }

        if (_objectPoolsRoot == null || _combatTextRoot == null || _settingsView == null || !_settingsView.IsConfigured ||
            _exitConfirmationView == null || !_exitConfirmationView.IsConfigured ||
            _settingsInput == null || _rewardSelectionView == null || _rearrangementView == null || !_rearrangementView.IsConfigured ||
            _waveHUDView == null ||
            _goldHUDView == null || !_goldHUDView.IsConfigured ||
            _coinsHUDView == null || !_coinsHUDView.IsConfigured ||
            _scoreHUDView == null || !_scoreHUDView.IsConfigured ||
            _rewardHUDView == null || _coreGoldUpgradeView == null ||
            _towerGoldUpgradeView == null || !_coreGoldUpgradeView.IsConfigured ||
            !_towerGoldUpgradeView.IsConfigured || _gameSpeedView == null || !_gameSpeedView.IsConfigured ||
            _preparationAutoSkipView == null || !_preparationAutoSkipView.IsConfigured ||
            _flyingGemPrefab == null || !_flyingGemPrefab.IsConfigured ||
            _flyingGemPoolRoot == null || _flyingGemSettings == null ||
            _gameStartData.PlayerLevelData == null)
        {
            Debug.LogError("게임 UI 참조가 설정되지 않았습니다.", this);
            return;
        }

        if (_gameStartData.WaveGenerationSettings == null)
        {
            Debug.LogError("WaveGenerationSettings가 설정되지 않았습니다.", this);
            return;
        }

        WavePlanGenerator wavePlanGenerator;

        try
        {
            wavePlanGenerator = new WavePlanGenerator(_gameStartData.WaveGenerationSettings);
        }
        catch (System.Exception exception)
        {
            Debug.LogException(exception, _gameStartData.WaveGenerationSettings);
            return;
        }

        Canvas.ForceUpdateCanvases();
        var combatBoundsProvider = new CombatBoundsProvider(
            _mainCamera,
            _coreGoldUpgradeView.LayoutRect);
        var combatLayoutCoordinator = new CombatLayoutCoordinator(
            combatBoundsProvider,
            _coreLayout);
        combatLayoutCoordinator.RefreshNow();

        var gameStateManager = new GameStateManager();

        var coreBoard = new CoreBoard();
        var coreManager = new CoreManager(coreBoard);

        coreBoundary.Initialize(coreManager);
        coreVisual.Initialize(coreManager);

        var addressableProvider = new AddressableProvider();
        var objectPoolManager = new ObjectPoolManager(
            addressableProvider,
            _objectPoolsRoot,
            _combatTextRoot);

        await objectPoolManager.InitializeAsync();

        var combatTextManager = new CombatTextManager(
            objectPoolManager.GetPool<FloatingCombatText>(),
            _combatTextRoot,
            _mainCamera);
        var gemsWallet = new GemsWallet();
        _flyingGemService = new FlyingGemService(
            _flyingGemPrefab,
            _flyingGemPoolRoot,
            _flyingGemSettings,
            gameStateManager,
            _mainCamera,
            combatBoundsProvider,
            combatTextManager,
            gemsWallet);
        await _flyingGemService.InitializeAsync();
        var projectileManager = new ProjectileManager(objectPoolManager.GetPool<Projectile>());
        var explosionEffectManager = new ExplosionEffectManager(objectPoolManager.GetPool<ExplosionEffect>());
        var laserManager = new LaserManager(
            objectPoolManager.GetPool<Laser>(),
            combatBoundsProvider);

        var cameraBoundsProvider = new CameraBoundsProvider(_mainCamera);
        var enemyFactory = new EnemyFactory(
            objectPoolManager.GetPool<Enemy>(),
            projectileManager,
            coreBoundary,
            cameraBoundsProvider,
            combatTextManager);
        var enemyManager = new EnemyManager(enemyFactory);
        var enemyQuery = new EnemyQuery(enemyManager);

        _goldManager = new GoldManager();
        _enemyGoldRewardService = new EnemyGoldRewardService(enemyManager, _goldManager);
        var playerProgression = new PlayerProgression(_gameStartData.PlayerLevelData);
        _enemyExperienceRewardService = new EnemyExperienceRewardService(enemyManager, playerProgression);

        var towerTargetSelector = new TowerTargetSelector(enemyQuery, combatBoundsProvider);
        var attackDependencies = new TowerAttackDependencies(projectileManager, explosionEffectManager, laserManager);
        var persistentCurrencyWallet = new PersistentCurrencyWallet();
        var persistentUpgradeService = new PersistentUpgradeService(
            _gameStartData.PersistentUpgradeCatalog,
            persistentCurrencyWallet);
        var globalStatModifiers = new TowerStatModifiers();
        var coreGoldStatModifiers = new TowerStatModifiers();
        var towerTypeUpgradeState = new TowerTypeUpgradeState(_gameStartData.TowerTypeUpgradeDataList);

        if (!persistentUpgradeService.TryLoadModifiers(
                PermanentUpgradeTarget.Core,
                TowerType.Basic,
                globalStatModifiers))
        {
            Debug.LogError("Failed to load CORE persistent upgrade modifiers.", _gameStartData);
            return;
        }

        foreach (TowerType towerType in System.Enum.GetValues(typeof(TowerType)))
        {
            if (persistentUpgradeService.TryLoadModifiers(
                    PermanentUpgradeTarget.Tower,
                    towerType,
                    towerTypeUpgradeState.GetStatModifiers(towerType)))
            {
                continue;
            }

            Debug.LogError(
                $"Failed to load persistent upgrade modifiers for {towerType}.",
                _gameStartData);
            return;
        }

        var towerFactory = new TowerFactory(
            towerTargetSelector,
            attackDependencies,
            globalStatModifiers,
            coreGoldStatModifiers,
            towerTypeUpgradeState);
        _towerManager = new TowerManager(
            towerFactory,
            coreManager,
            _coreLayout,
            globalStatModifiers,
            coreGoldStatModifiers,
            towerTypeUpgradeState);
        TowerManager towerManager = _towerManager;

        var coreGoldUpgradeManager = new CoreGoldUpgradeManager(
            _goldManager,
            gameStateManager,
            towerManager,
            _gameStartData.CoreGoldUpgradeDataList);

        for (int i = 0; i < coreGoldUpgradeManager.ConfigurationErrors.Count; i++)
            Debug.LogWarning(coreGoldUpgradeManager.ConfigurationErrors[i], _gameStartData);

        _towerGoldUpgradeManager = new TowerGoldUpgradeManager(
            _goldManager,
            gameStateManager,
            towerManager,
            _gameStartData.TowerGoldUpgradeDataList);
        _towerSelectionManager = new TowerSelectionManager(gameStateManager, towerManager);

        for (int i = 0; i < _towerGoldUpgradeManager.ConfigurationErrors.Count; i++)
            Debug.LogWarning(_towerGoldUpgradeManager.ConfigurationErrors[i], _gameStartData);

        CoreSlotEnhancementSelectionController slotEnhancementSelection =
            GetComponent<CoreSlotEnhancementSelectionController>();

        if (slotEnhancementSelection == null)
            slotEnhancementSelection = gameObject.AddComponent<CoreSlotEnhancementSelectionController>();

        slotEnhancementSelection.Initialize(gameStateManager, coreManager, coreVisual, _mainCamera);

        var rewardDependencies = new RewardDependencies(
            towerManager,
            _goldManager,
            slotEnhancementSelection
        );

        if (!_gameStartData.RewardSelectionSettings.TryValidate(out string rewardSettingsError))
        {
            Debug.LogError(rewardSettingsError, _gameStartData);
            return;
        }

        var rewardSelectionManager = new RewardSelectionManager(
            _gameStartData.RewardDataList,
            rewardDependencies,
            _gameStartData.RewardSelectionSettings
        );

        var spawnPositionCalculator = new SpawnPositionCalculator();
        var enemySpawnPositionProvider = new EnemySpawnPositionProvider(
            _coreLayout,
            combatBoundsProvider,
            spawnPositionCalculator,
            _enemySpawnMargin);

        var spawnManager = new SpawnManager(enemyFactory, enemyManager, enemySpawnPositionProvider, _coreLayout.transform);

        _deathBehaviorService = new EnemyDeathBehaviorService(enemyManager, spawnManager);

        var waveManager = new WaveManager(spawnManager, enemyManager);
        var preparationAutoSkipSetting = new PreparationAutoSkipSetting();
        _gameManager = new GameManager(
            gameStateManager,
            waveManager,
            rewardSelectionManager,
            wavePlanGenerator,
            _gameStartData.WaveGenerationSettings,
            playerProgression,
            towerManager,
            preparationAutoSkipSetting);
        GameManager gameManager = _gameManager;

        var gameEndRewardPolicy = new GameEndRewardPolicy();
        _runScoreCounter = new RunScoreCounter(enemyManager, gameEndRewardPolicy);
        var gameSpeedAccess = new GameSpeedAccess(hasRemoveAdsEntitlement: false);
        _gameTimePauseService = new GameTimePauseService(gameStateManager, gameSpeedAccess);
        var combatCleanupService = new GameCombatCleanupService(
            enemyManager,
            projectileManager,
            laserManager,
            explosionEffectManager);
        _gameEndFlowManager = new GameEndFlowManager(
            gameStateManager,
            gameManager,
            towerManager,
            _runScoreCounter,
            gameEndRewardPolicy,
            persistentCurrencyWallet,
            new MockRewardedAdService(),
            combatCleanupService,
            _gameTimePauseService,
            _earlyExitCoinsPenaltyRate);

        GameEndView gameEndView = GetComponent<GameEndView>();

        if (gameEndView == null)
            gameEndView = gameObject.AddComponent<GameEndView>();

        _gameEndPresenter = new GameEndPresenter(
            gameEndView,
            _gameEndFlowManager,
            gameStateManager);
        _settingsPresenter = new BattleSettingsPresenter(
            _settingsView,
            _exitConfirmationView,
            _settingsInput,
            gameStateManager,
            _gameEndFlowManager);
        _gameSpeedPresenter = new GameSpeedPresenter(
            _gameSpeedView,
            _gameTimePauseService,
            gameStateManager);
        _preparationAutoSkipPresenter = new PreparationAutoSkipPresenter(
            _preparationAutoSkipView,
            preparationAutoSkipSetting,
            gameStateManager);

        towerManager.AllTowersDestroyed += gameManager.GameOver;
        towerSpawnTester.Initialize(towerManager);

        bool isPlaced = await towerManager.TryPlaceTowerAsync(_gameStartData.InitialTowerSlotNumber, _gameStartData.BasicTowerPartData);

        if (!isPlaced)
        {
            Debug.LogError("초기 타워 배치에 실패했습니다.", this);
            return;
        }

        _rewardSelectionPresenter = new RewardSelectionPresenter(_rewardSelectionView, gameStateManager, gameManager, rewardSelectionManager);
        _rearrangementPresenter = new CoreRearrangementPresenter(_rearrangementView, gameStateManager, gameManager);
        _waveHUDPresenter = new WaveHUDPresenter(_waveHUDView, gameManager, gameStateManager, playerProgression);
        _goldHUDPresenter = new GoldHUDPresenter(_goldHUDView, _goldManager);
        _coinsHUDPresenter = new CoinsHUDPresenter(_coinsHUDView, gameManager, gameEndRewardPolicy);
        _scoreHUDPresenter = new ScoreHUDPresenter(_scoreHUDView, _runScoreCounter);
        _rewardHUDPresenter = new RewardHUDPresenter(_rewardHUDView, playerProgression);
        _coreGoldUpgradePresenter = new CoreGoldUpgradePresenter(
            _coreGoldUpgradeView,
            coreGoldUpgradeManager,
            _goldManager,
            gameStateManager);
        _towerGoldUpgradePresenter = new TowerGoldUpgradePresenter(
            _towerGoldUpgradeView,
            _towerGoldUpgradeManager,
            _towerSelectionManager,
            towerManager,
            _goldManager,
            gameStateManager);

        TowerGoldUpgradeInput towerGoldUpgradeInput = GetComponent<TowerGoldUpgradeInput>();

        if (towerGoldUpgradeInput == null)
            towerGoldUpgradeInput = gameObject.AddComponent<TowerGoldUpgradeInput>();

        towerGoldUpgradeInput.Initialize(
            gameStateManager,
            coreManager,
            coreVisual,
            _towerSelectionManager,
            _mainCamera,
            _flyingGemService);

        CoreRearrangementInput rearrangementInput = GetComponent<CoreRearrangementInput>();

        if (rearrangementInput == null)
            rearrangementInput = gameObject.AddComponent<CoreRearrangementInput>();

        rearrangementInput.Initialize(gameStateManager, coreManager, towerManager, coreVisual, _mainCamera);

#if UNITY_EDITOR
        TowerCombatDebugPanel combatDebugPanel = GetComponent<TowerCombatDebugPanel>();

        if (combatDebugPanel == null)
            combatDebugPanel = gameObject.AddComponent<TowerCombatDebugPanel>();

        combatDebugPanel.Initialize(
            coreManager,
            globalStatModifiers,
            rewardSelectionManager,
            _gameStartData.RewardDataList
        );
#endif

        Register(projectileManager);
        Register(explosionEffectManager);
        Register(laserManager);
        Register(combatTextManager);
        Register(combatLayoutCoordinator);
        Register(_flyingGemService);
        Register(gameManager);

        Canvas.ForceUpdateCanvases();

        SceneTransition sceneTransition = SceneTransition.Instance;

        if (sceneTransition != null && sceneTransition.IsTransitioning)
            await sceneTransition.FadeInAsync();

        await WaitForBattleStartDelayAsync();
        gameManager.StartGame();
        _gameSpeedPresenter.Synchronize();
        _preparationAutoSkipPresenter.Synchronize();
        Canvas.ForceUpdateCanvases();
    }

    private UniTask WaitForBattleStartDelayAsync()
    {
        if (_battleStartDelay <= 0f)
            return UniTask.CompletedTask;

        return UniTask.Delay(
            System.TimeSpan.FromSeconds(_battleStartDelay),
            DelayType.UnscaledDeltaTime,
            cancellationToken: this.GetCancellationTokenOnDestroy());
    }

    protected override void OnDestroy()
    {
        if (_towerManager != null && _gameManager != null)
            _towerManager.AllTowersDestroyed -= _gameManager.GameOver;

        _settingsPresenter?.Dispose();
        _gameSpeedPresenter?.Dispose();
        _preparationAutoSkipPresenter?.Dispose();
        _gameEndPresenter?.Dispose();
        _gameEndFlowManager?.Dispose();
        _gameTimePauseService?.Dispose();
        _runScoreCounter?.Dispose();
        _flyingGemService?.Dispose();
        _towerGoldUpgradePresenter?.Dispose();
        _coreGoldUpgradePresenter?.Dispose();
        _rewardHUDPresenter?.Dispose();
        _scoreHUDPresenter?.Dispose();
        _coinsHUDPresenter?.Dispose();
        _goldHUDPresenter?.Dispose();
        _waveHUDPresenter?.Dispose();
        _rearrangementPresenter?.Dispose();
        _rewardSelectionPresenter?.Dispose();

        _deathBehaviorService?.Dispose();
        _gameManager?.Dispose();
        _enemyGoldRewardService?.Dispose();
        _enemyExperienceRewardService?.Dispose();
        _towerSelectionManager?.Dispose();
        _towerGoldUpgradeManager?.Dispose();
        _towerManager?.Dispose();

        base.OnDestroy();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        _flyingGemSettings?.Validate();
        _enemySpawnMargin = Mathf.Max(0f, _enemySpawnMargin);
        _battleStartDelay = Mathf.Max(0f, _battleStartDelay);
        _earlyExitCoinsPenaltyRate = Mathf.Clamp01(_earlyExitCoinsPenaltyRate);
    }
#endif
}
