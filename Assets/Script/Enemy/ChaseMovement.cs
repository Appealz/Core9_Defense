using UnityEngine;

public class ChaseMovement : IEnemyMovement
{
    public void Move(Transform transform, Vector3 targetPosition, float deltaTime, float moveSpeed)
    {
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * deltaTime);
    }
}
