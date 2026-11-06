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
    public void PausedState_StopsTime_AndPlayingRestoresSelectedSpeed()
    {
        var stateManager = new GameStateManager();
        using var service = new GameTimePauseService(stateManager, new FakeSpeedAccess(true));

        stateManager.ChangeState(GameState.Playing);
        Assert.That(service.TryIncreaseSpeed(), Is.True);
        Assert.That(Time.timeScale, Is.EqualTo(2f));

        stateManager.ChangeState(GameState.Paused);
        Assert.That(Time.timeScale, Is.Zero);

        stateManager.ChangeState(GameState.Playing);
        Assert.That(Time.timeScale, Is.EqualTo(2f));
        Assert.That(service.CurrentSpeed, Is.EqualTo(2));
    }

    [Test]
    public void LockedUser_CanUseOneAndTwoButNotTripleSpeed()
    {
        var stateManager = new GameStateManager();
        using var service = new GameTimePauseService(stateManager, new FakeSpeedAccess(false));
        stateManager.ChangeState(GameState.Playing);

        Assert.That(service.CurrentSpeed, Is.EqualTo(1));
        Assert.That(service.TryIncreaseSpeed(), Is.True);
        Assert.That(service.CurrentSpeed, Is.EqualTo(2));
        Assert.That(service.TryIncreaseSpeed(), Is.False);
        Assert.That(service.CurrentSpeed, Is.EqualTo(2));
        Assert.That(service.IsTripleSpeedLocked, Is.True);
    }

    [Test]
    public void UnlockedUser_CanCycleBetweenFixedSpeedSteps()
    {
        var stateManager = new GameStateManager();
        using var service = new GameTimePauseService(stateManager, new FakeSpeedAccess(true));
        stateManager.ChangeState(GameState.Playing);

        Assert.That(service.TryIncreaseSpeed(), Is.True);
        Assert.That(service.TryIncreaseSpeed(), Is.True);
        Assert.That(service.CurrentSpeed, Is.EqualTo(3));
        Assert.That(Time.timeScale, Is.EqualTo(3f));
        Assert.That(service.TryIncreaseSpeed(), Is.False);
        Assert.That(service.TryDecreaseSpeed(), Is.True);
        Assert.That(service.TryDecreaseSpeed(), Is.True);
        Assert.That(service.CurrentSpeed, Is.EqualTo(1));
        Assert.That(service.TryDecreaseSpeed(), Is.False);
    }

    [Test]
    public void SpeedChange_OutsidePlaying_IsRejected()
    {
        var stateManager = new GameStateManager();
        using var service = new GameTimePauseService(stateManager, new FakeSpeedAccess(true));

        Assert.That(service.TryIncreaseSpeed(), Is.False);
        Assert.That(service.CurrentSpeed, Is.EqualTo(1));
        Assert.That(Time.timeScale, Is.EqualTo(1f));
    }

    [Test]
    public void ResetToDefault_RestoresOneTimesSpeed()
    {
        var stateManager = new GameStateManager();
        using var service = new GameTimePauseService(stateManager, new FakeSpeedAccess(true));
        stateManager.ChangeState(GameState.Playing);
        service.TryIncreaseSpeed();
        service.TryIncreaseSpeed();

        service.ResetToDefault();

        Assert.That(service.CurrentSpeed, Is.EqualTo(1));
        Assert.That(Time.timeScale, Is.EqualTo(1f));
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

    [Test]
    public void GameOverContinue_RestoresSelectedSpeed()
    {
        var stateManager = new GameStateManager();
        using var service = new GameTimePauseService(stateManager, new FakeSpeedAccess(true));
        stateManager.ChangeState(GameState.Playing);
        service.TryIncreaseSpeed();
        service.TryIncreaseSpeed();

        stateManager.ChangeState(GameState.GameOver);
        Assert.That(Time.timeScale, Is.Zero);

        stateManager.ChangeState(GameState.Playing);
        Assert.That(Time.timeScale, Is.EqualTo(3f));
        Assert.That(service.CurrentSpeed, Is.EqualTo(3));
    }

    private sealed class FakeSpeedAccess : IGameSpeedAccess
    {
        public bool CanUseTripleSpeed { get; }

        public FakeSpeedAccess(bool canUseTripleSpeed)
        {
            CanUseTripleSpeed = canUseTripleSpeed;
        }
    }
}
