using NUnit.Framework;
using UnityEngine;

public sealed class GameTimePauseServiceTests
{
    private float _originalTimeScale;

    [SetUp]
    public void SetUp()
    {
        _originalTimeScale = Time.timeScale;
        Time.timeScale = 1f;
    }

    [TearDown]
    public void TearDown()
    {
        Time.timeScale = _originalTimeScale;
    }

    [Test]
    public void PausedState_StopsTime_AndPlayingRestoresPreviousScale()
    {
        var stateManager = new GameStateManager();
        using var service = new GameTimePauseService(stateManager);

        stateManager.ChangeState(GameState.Playing);
        Time.timeScale = 0.75f;

        stateManager.ChangeState(GameState.Paused);
        Assert.That(Time.timeScale, Is.Zero);

        stateManager.ChangeState(GameState.Playing);
        Assert.That(Time.timeScale, Is.EqualTo(0.75f));
    }

    [Test]
    public void ExternalPause_IsNotOwnedOrReleased()
    {
        var stateManager = new GameStateManager();
        using var service = new GameTimePauseService(stateManager);

        stateManager.ChangeState(GameState.Playing);
        Time.timeScale = 0f;

        stateManager.ChangeState(GameState.Paused);
        stateManager.ChangeState(GameState.Playing);

        Assert.That(Time.timeScale, Is.Zero);
    }

    [Test]
    public void LeavingPausedForNonPlayingState_RestoresOwnedTimeScale()
    {
        var stateManager = new GameStateManager();
        using var service = new GameTimePauseService(stateManager);

        stateManager.ChangeState(GameState.Playing);
        stateManager.ChangeState(GameState.Paused);
        stateManager.ChangeState(GameState.RewardSelection);

        Assert.That(Time.timeScale, Is.EqualTo(1f));
    }

    [Test]
    public void PausedToGameOver_KeepsOwnedPauseUntilFlowRestoresIt()
    {
        var stateManager = new GameStateManager();
        using var service = new GameTimePauseService(stateManager);

        stateManager.ChangeState(GameState.Playing);
        stateManager.ChangeState(GameState.Paused);
        stateManager.ChangeState(GameState.GameOver);

        Assert.That(Time.timeScale, Is.Zero);

        service.Restore();
        Assert.That(Time.timeScale, Is.EqualTo(1f));
    }
}
