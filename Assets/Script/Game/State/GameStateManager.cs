using System;

public enum GameState
{
    Initializing,
    Playing,
    RewardSelection,
    Rearrangement,
    Paused,
    Victory,
    GameOver
}

public sealed class GameStateManager
{
    public GameState CurrentState { get; private set; }

    public event Action<GameState, GameState> StateChanged;

    public GameStateManager()
    {
        CurrentState = GameState.Initializing;
    }

    public void ChangeState(GameState nextState)
    {
        if (CurrentState == nextState)
            return;

        GameState previousState = CurrentState;

        CurrentState = nextState;

        StateChanged?.Invoke(previousState, nextState);
    }
}