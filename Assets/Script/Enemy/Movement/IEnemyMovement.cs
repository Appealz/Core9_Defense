using UnityEngine;

public interface IEnemyMovement
{
    void Move(Transform transform, Vector3 targetPosition, float deltaTime, float moveSpeed);
}

