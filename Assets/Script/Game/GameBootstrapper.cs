using UnityEngine;

public sealed class GameBootstrapper : SceneBootstrapper
{
    protected override void Compose()
    {
        var gameStateManager = new GameStateManager();
        var gameManager = new GameManager(gameStateManager);

        Register(gameManager);

        gameManager.StartGame();
    }    
}
