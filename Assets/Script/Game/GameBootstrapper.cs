using Cysharp.Threading.Tasks;
using UnityEngine;

public sealed class GameBootstrapper : SceneBootstrapper
{
    [SerializeField] private GameStartData _gameStartData;
    [SerializeField] private CoreLayout _coreLayout;

    protected override async UniTask ComposeAsync()
    {
        var gameStateManager = new GameStateManager();
        var gameManager = new GameManager(gameStateManager);

        var coreBoard = new CoreBoard();
        var coreManager = new CoreManager(coreBoard);

        var towerFactory = new TowerFactory();
        var towerManager = new TowerManager(towerFactory, coreManager, _coreLayout);

        bool isPlaced = await towerManager.TryPlaceTowerAsync(
            _gameStartData.InitialTowerSlotNumber,
            _gameStartData.BasicTowerPartData);

        if (!isPlaced)
        {
            Debug.LogError("초기 타워 배치에 실패했습니다.", this);
            return;
        }

        Register(gameManager);
        gameManager.StartGame();
    }
}