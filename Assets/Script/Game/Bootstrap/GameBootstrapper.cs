
using Cysharp.Threading.Tasks;
using UnityEngine;

public sealed class GameBootstrapper : SceneBootstrapper
{
    [SerializeField] private GameStartData _gameStartData;
    [SerializeField] private CoreLayout _coreLayout;
    [SerializeField] private Camera _mainCamera;
    [SerializeField] private RewardSelectionView _rewardSelectionView;
    [SerializeField] private CoreRearrangementView _rearrangementView;
    [SerializeField] private WaveHUDView _waveHUDView;
    [SerializeField] private GoldHUDView _goldHUDView;
    [SerializeField] private RewardHUDView _rewardHUDView;
    [SerializeField] private CoreGoldUpgradeView _coreGoldUpgradeView;
    [SerializeField] private TowerGoldUpgradeView _towerGoldUpgradeView;
    [SerializeField] private float _enemySpawnMargin = 1f;

    private RewardSelectionPresenter _rewardSelectionPresenter;
    private CoreRearrangementPresenter _rearrangementPresenter;
    private WaveHUDPresenter _waveHUDPresenter;
    private GoldHUDPresenter _goldHUDPresenter;
    private RewardHUDPresenter _rewardHUDPresenter;
    private CoreGoldUpgradePresenter _coreGoldUpgradePresenter;
    private TowerGoldUpgradePresenter _towerGoldUpgradePresenter;
    private TowerGoldUpgradeManager _towerGoldUpgradeManager;
    private TowerSelectionManager _towerSelectionManager;
    private TowerManager _towerManager;
    private GameManager _gameManager;

    private GoldManager _goldManager;
    private EnemyGoldRewardService _enemyGoldRewardService;
    private EnemyExperienceRewardService _enemyExperienceRewardService;
    private EnemyDeathBehaviorService _deathBehaviorService;

    protected override async UniTask ComposeAsync()
    {
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

        if (_rewardSelectionView == null || _rearrangementView == null || _waveHUDView == null ||
            _goldHUDView == null || _rewardHUDView == null || _coreGoldUpgradeView == null ||
            _towerGoldUpgradeView == null || !_coreGoldUpgradeView.IsConfigured ||
            !_towerGoldUpgradeView.IsConfigured || _gameStartData.PlayerLevelData == null)
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
        var objectPoolManager = new ObjectPoolManager(addressableProvider);

        await objectPoolManager.InitializeAsync();

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
            cameraBoundsProvider);
        var enemyManager = new EnemyManager(enemyFactory);
        var enemyQuery = new EnemyQuery(enemyManager);

        _goldManager = new GoldManager();
        _enemyGoldRewardService = new EnemyGoldRewardService(enemyManager, _goldManager);
        var playerProgression = new PlayerProgression(_gameStartData.PlayerLevelData);
        _enemyExperienceRewardService = new EnemyExperienceRewardService(enemyManager, playerProgression);

        var towerTargetSelector = new TowerTargetSelector(enemyQuery, combatBoundsProvider);
        var attackDependencies = new TowerAttackDependencies(projectileManager, explosionEffectManager, laserManager);
        var globalStatModifiers = new TowerStatModifiers();
        var coreGoldStatModifiers = new TowerStatModifiers();
        var towerTypeUpgradeState = new TowerTypeUpgradeState(_gameStartData.TowerTypeUpgradeDataList);

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
        _gameManager = new GameManager(
            gameStateManager,
            waveManager,
            rewardSelectionManager,
            wavePlanGenerator,
            _gameStartData.WaveGenerationSettings,
            playerProgression);
        GameManager gameManager = _gameManager;

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
            _mainCamera);

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
        Register(combatLayoutCoordinator);
        Register(gameManager);

        gameManager.StartGame();
    }

    protected override void OnDestroy()
    {
        _towerGoldUpgradePresenter?.Dispose();
        _coreGoldUpgradePresenter?.Dispose();
        _rewardHUDPresenter?.Dispose();
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
}
