using UnityEngine;

public sealed class SpawnPositionCalculator
{
    public Vector2 Calculate(
        CameraBounds bounds,
        Vector2 origin,
        Vector2 direction,
        float margin)
    {
        direction.Normalize();

        float distanceX = float.PositiveInfinity;
        float distanceY = float.PositiveInfinity;

        if (Mathf.Abs(direction.x) > Mathf.Epsilon)
        {
            distanceX = direction.x > 0f
                ? (bounds.Right - origin.x) / direction.x
                : (bounds.Left - origin.x) / direction.x;
        }

        if (Mathf.Abs(direction.y) > Mathf.Epsilon)
        {
            distanceY = direction.y > 0f
                ? (bounds.Top - origin.y) / direction.y
                : (bounds.Bottom - origin.y) / direction.y;
        }

        float distanceToEdge =
            Mathf.Min(distanceX, distanceY);

        return origin +
               direction * (distanceToEdge + margin);
    }
}