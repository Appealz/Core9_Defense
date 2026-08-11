using System;

public sealed class GameManager : IUpdatable
{
    private readonly GameStateManager _gameStateManager;

    public GameManager(GameStateManager gameStateManager)
    {
        _gameStateManager = gameStateManager;
    }

    public void StartGame()
    {
        if (_gameStateManager.CurrentState != GameState.Initializing)
            return;

        _gameStateManager.ChangeState(GameState.Playing);
    }

    public void Update(float deltaTime)
    {
        if (_gameStateManager.CurrentState != GameState.Playing)
            return;

        UpdatePlaying(deltaTime);
    }

    private void UpdatePlaying(float deltaTime)
    {
    }
}