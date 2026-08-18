using UnityEngine;

public sealed class GameBootstrapper : SceneBootstrapper
{
    [SerializeField] private GameStartData _gameStartData;
    protected override void Compose()
    {
        var gameStateManager = new GameStateManager();
        var gameManager = new GameManager(gameStateManager);

        var coreBoard = new CoreBoard();
        var coreManager = new CoreManager(coreBoard);

        var towerFactory = new TowerFactory();
        var towerManager = new TowerManager(towerFactory, coreManager);

        towerManager.TryPlaceTower(_gameStartData.InitialTowerSlotNumber, _gameStartData.BasicTowerPartData);

        Register(gameManager);
        gameManager.StartGame();
    }    
}
