using Cysharp.Threading.Tasks;
using UnityEngine;

public sealed class GameBootstrapper : SceneBootstrapper
{
    [SerializeField] private GameStartData _gameStartData;
    [SerializeField] private CoreLayout _coreLayout;
    [SerializeField] private Camera _mainCamera;
    [SerializeField] private float _enemySpawnMargin = 1f;

    protected override async UniTask ComposeAsync()
    {
        var gameStateManager = new GameStateManager();

        var coreBoard = new CoreBoard();
        var coreManager = new CoreManager(coreBoard);

        var towerFactory = new TowerFactory();
        var towerManager = new TowerManager(towerFactory, coreManager, _coreLayout);

        var addressableProvider = new AddressableProvider();
        var objectPoolManager = new ObjectPoolManager(addressableProvider);

        await objectPoolManager.InitializeAsync();

        var enemyFactory = new EnemyFactory(objectPoolManager.GetPool<Enemy>());
        var enemyManager = new EnemyManager(enemyFactory);

        var cameraBoundsProvider = new CameraBoundsProvider(_mainCamera);
        var spawnPositionCalculator = new SpawnPositionCalculator();
        var enemySpawnPositionProvider = new EnemySpawnPositionProvider(_coreLayout, cameraBoundsProvider, spawnPositionCalculator, _enemySpawnMargin);

        var spawnManager = new SpawnManager(enemyFactory, enemyManager, enemySpawnPositionProvider, _coreLayout.transform);
        var waveManager = new WaveManager(spawnManager, enemyManager);

        var gameManager = new GameManager(gameStateManager, waveManager, _gameStartData.WaveDataList);

        bool isPlaced = await towerManager.TryPlaceTowerAsync(_gameStartData.InitialTowerSlotNumber, _gameStartData.BasicTowerPartData);

        if (!isPlaced)
        {
            Debug.LogError("초기 타워 배치에 실패했습니다.", this);
            return;
        }

        if (_gameStartData.WaveDataList.Count == 0)
        {
            Debug.LogError("WaveDataList가 비어 있습니다.", this);
            return;
        }

        Register(gameManager);
        gameManager.StartGame();
    }
}