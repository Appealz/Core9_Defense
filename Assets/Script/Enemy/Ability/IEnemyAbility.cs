public interface IEnemyAbility
{
    void AbilityUpdate(float deltaTime, bool isMovementStopped);
    void Stop();
}
