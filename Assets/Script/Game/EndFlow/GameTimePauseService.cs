using System;
using UnityEngine;

public sealed class GameTimePauseService : IDisposable
{
    private readonly GameStateManager _gameStateManager;
    private float _timeScaleBeforePause = 1f;
    private bool _ownsPause;

    public GameTimePauseService(GameStateManager gameStateManager)
    {
        _gameStateManager = gameStateManager ?? throw new ArgumentNullException(nameof(gameStateManager));
        _gameStateManager.StateChanged += HandleStateChanged;
    }

    private void HandleStateChanged(GameState previousState, GameState nextState)
    {
        if (nextState == GameState.GameOver)
        {
            Pause();
            return;
        }

        if (nextState == GameState.Playing)
            Restore();
    }

    private void Pause()
    {
        if (_ownsPause)
            return;

        _timeScaleBeforePause = Time.timeScale;
        Time.timeScale = 0f;
        _ownsPause = true;
    }

    public void Restore()
    {
        if (!_ownsPause)
            return;

        Time.timeScale = _timeScaleBeforePause <= 0f ? 1f : _timeScaleBeforePause;
        _ownsPause = false;
    }

    public void Dispose()
    {
        _gameStateManager.StateChanged -= HandleStateChanged;
        Restore();
    }
}
