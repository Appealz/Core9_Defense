using System;
using UnityEngine;

public sealed class GameTimePauseService : IDisposable
{
    private static readonly int[] SpeedSteps = { 1, 2, 3 };

    private readonly GameStateManager _gameStateManager;
    private readonly IGameSpeedAccess _speedAccess;

    private int _speedIndex;
    private bool _ownsPause;

    public event Action<int> SpeedChanged;

    public int CurrentSpeed => SpeedSteps[_speedIndex];
    public bool CanDecreaseSpeed => _speedIndex > 0;
    public bool CanIncreaseSpeed => _speedIndex < MaximumSpeedIndex;
    public bool IsTripleSpeedLocked => !_speedAccess.CanUseTripleSpeed;

    private int MaximumSpeedIndex => _speedAccess.CanUseTripleSpeed
        ? SpeedSteps.Length - 1
        : SpeedSteps.Length - 2;

    public GameTimePauseService(
        GameStateManager gameStateManager,
        IGameSpeedAccess speedAccess = null)
    {
        _gameStateManager = gameStateManager ?? throw new ArgumentNullException(nameof(gameStateManager));
        _speedAccess = speedAccess ?? new GameSpeedAccess(false);
        _gameStateManager.StateChanged += HandleStateChanged;
        ResetToDefault();
    }

    private void HandleStateChanged(GameState previousState, GameState nextState)
    {
        if (nextState == GameState.GameOver || nextState == GameState.Paused)
        {
            Pause();
            return;
        }

        if (nextState == GameState.Playing || previousState == GameState.Paused)
            Restore();
    }

    private void Pause()
    {
        if (_ownsPause || Time.timeScale <= 0f)
            return;

        Time.timeScale = 0f;
        _ownsPause = true;
    }

    public bool TryIncreaseSpeed()
    {
        return TrySetSpeedIndex(_speedIndex + 1);
    }

    public bool TryDecreaseSpeed()
    {
        return TrySetSpeedIndex(_speedIndex - 1);
    }

    private bool TrySetSpeedIndex(int speedIndex)
    {
        if (_gameStateManager.CurrentState != GameState.Playing ||
            Time.timeScale <= 0f || speedIndex < 0 || speedIndex > MaximumSpeedIndex ||
            speedIndex == _speedIndex)
        {
            return false;
        }

        _speedIndex = speedIndex;
        Time.timeScale = CurrentSpeed;
        SpeedChanged?.Invoke(CurrentSpeed);
        return true;
    }

    public void Restore()
    {
        if (!_ownsPause)
            return;

        Time.timeScale = CurrentSpeed;
        _ownsPause = false;
    }

    public void ResetToDefault()
    {
        bool speedChanged = _speedIndex != 0;
        _speedIndex = 0;
        _ownsPause = false;
        Time.timeScale = SpeedSteps[0];

        if (speedChanged)
            SpeedChanged?.Invoke(CurrentSpeed);
    }

    public void Dispose()
    {
        _gameStateManager.StateChanged -= HandleStateChanged;
        ResetToDefault();
    }
}
