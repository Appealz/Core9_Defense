
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
    [SerializeField] private float _enemySpawnMargin = 1f;

    private RewardSelectionPresenter _rewardSelectionPresenter;
    private CoreRearrangementPresenter _rearrangementPresenter;
    private WaveHUDPresenter _waveHUDPresenter;
    private GoldHUDPresenter _goldHUDPresenter;

    private GoldManager _goldManager;
    private EnemyGoldRewardService _enemyGoldRewardService;
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

        if (_rewardSelectionView == null || _rearrangementView == null || _waveHUDView == null || _goldHUDView == null)
        {
            Debug.LogError("게임 UI 참조가 설정되지 않았습니다.", this);
            return;
        }

        if (_gameStartData.WaveDataList.Count == 0)
        {
            Debug.LogError("WaveDataList가 비어 있습니다.", this);
            return;
        }

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
        var laserManager = new LaserManager(objectPoolManager.GetPool<Laser>());

        var enemyFactory = new EnemyFactory(objectPoolManager.GetPool<Enemy>(), projectileManager, coreBoundary);
        var enemyManager = new EnemyManager(enemyFactory);
        var enemyQuery = new EnemyQuery(enemyManager);

        _goldManager = new GoldManager();
        _enemyGoldRewardService = new EnemyGoldRewardService(enemyManager, _goldManager);

        var towerTargetSelector = new TowerTargetSelector(enemyQuery);
        var attackDependencies = new TowerAttackDependencies(projectileManager, explosionEffectManager, laserManager);
        var globalStatModifiers = new TowerStatModifiers();

        var towerFactory = new TowerFactory(towerTargetSelector, attackDependencies, globalStatModifiers);
        var towerManager = new TowerManager(towerFactory, coreManager, _coreLayout, globalStatModifiers);

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

        var cameraBoundsProvider = new CameraBoundsProvider(_mainCamera);
        var spawnPositionCalculator = new SpawnPositionCalculator();
        var enemySpawnPositionProvider = new EnemySpawnPositionProvider(_coreLayout, cameraBoundsProvider, spawnPositionCalculator, _enemySpawnMargin);

        var spawnManager = new SpawnManager(enemyFactory, enemyManager, enemySpawnPositionProvider, _coreLayout.transform);

        _deathBehaviorService = new EnemyDeathBehaviorService(enemyManager, spawnManager);

        var waveManager = new WaveManager(spawnManager, enemyManager);
        var gameManager = new GameManager(gameStateManager, waveManager, rewardSelectionManager, _gameStartData.WaveDataList);

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
        _waveHUDPresenter = new WaveHUDPresenter(_waveHUDView, waveManager, gameManager, gameStateManager);
        _goldHUDPresenter = new GoldHUDPresenter(_goldHUDView, _goldManager);

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
        Register(gameManager);

        gameManager.StartGame();
    }

    protected override void OnDestroy()
    {
        _goldHUDPresenter?.Dispose();
        _waveHUDPresenter?.Dispose();
        _rearrangementPresenter?.Dispose();
        _rewardSelectionPresenter?.Dispose();

        _deathBehaviorService?.Dispose();
        _enemyGoldRewardService?.Dispose();

        base.OnDestroy();
    }
}
