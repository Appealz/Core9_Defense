using UnityEngine;

public class EnemyController
{
    private readonly EnemyStats _stats;
    private readonly IEnemyMovement _movement;

    private bool _isCoreContact;

    public EnemyController(EnemyStats stats, IEnemyMovement movement)
    {
        _stats = stats;
        _movement = movement;
    }

    public void EnemyUpdate(Transform transform, Vector3 targetPosition, float deltaTime)
    {
        if (_isCoreContact)
            return;

        _movement.Move(transform, targetPosition, deltaTime, _stats.MoveSpeed);
    }

    public void SetCoreContact(bool isContact)
    {
        _isCoreContact = isContact;
    }
}