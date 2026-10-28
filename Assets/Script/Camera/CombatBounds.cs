using UnityEngine;

public readonly struct CombatBounds
{
    private const float Epsilon = 0.0001f;

    public float Left { get; }
    public float Right { get; }
    public float Bottom { get; }
    public float Top { get; }
    public Vector2 Center => new((Left + Right) * 0.5f, (Bottom + Top) * 0.5f);
    public float Width => Right - Left;
    public float Height => Top - Bottom;

    public CombatBounds(float left, float right, float bottom, float top)
    {
        Left = Mathf.Min(left, right);
        Right = Mathf.Max(left, right);
        Bottom = Mathf.Min(bottom, top);
        Top = Mathf.Max(bottom, top);
    }

    public static CombatBounds FromCameraBounds(
        CameraBounds cameraBounds,
        float bottomViewportY)
    {
        float combatBottom = Mathf.Lerp(
            cameraBounds.Bottom,
            cameraBounds.Top,
            Mathf.Clamp01(bottomViewportY));

        return new CombatBounds(
            cameraBounds.Left,
            cameraBounds.Right,
            combatBottom,
            cameraBounds.Top);
    }

    public bool Contains(Vector2 point)
    {
        return point.x >= Left - Epsilon && point.x <= Right + Epsilon &&
               point.y >= Bottom - Epsilon && point.y <= Top + Epsilon;
    }

    public bool Intersects(Bounds worldBounds)
    {
        return worldBounds.max.x >= Left - Epsilon &&
               worldBounds.min.x <= Right + Epsilon &&
               worldBounds.max.y >= Bottom - Epsilon &&
               worldBounds.min.y <= Top + Epsilon;
    }

    public bool TryGetRayIntersection(
        Vector2 origin,
        Vector2 direction,
        out Vector2 intersection)
    {
        intersection = default;

        if (direction.sqrMagnitude <= Epsilon * Epsilon)
            return false;

        direction.Normalize();
        float enterDistance = float.NegativeInfinity;
        float exitDistance = float.PositiveInfinity;

        if (!TryClipAxis(origin.x, direction.x, Left, Right, ref enterDistance, ref exitDistance) ||
            !TryClipAxis(origin.y, direction.y, Bottom, Top, ref enterDistance, ref exitDistance) ||
            exitDistance < Mathf.Max(enterDistance, 0f))
        {
            return false;
        }

        float distance = Contains(origin) ? exitDistance : Mathf.Max(enterDistance, 0f);
        intersection = origin + direction * distance;
        intersection.x = Mathf.Clamp(intersection.x, Left, Right);
        intersection.y = Mathf.Clamp(intersection.y, Bottom, Top);
        return true;
    }

    private static bool TryClipAxis(
        float origin,
        float direction,
        float minimum,
        float maximum,
        ref float enterDistance,
        ref float exitDistance)
    {
        if (Mathf.Abs(direction) <= Epsilon)
            return origin >= minimum - Epsilon && origin <= maximum + Epsilon;

        float first = (minimum - origin) / direction;
        float second = (maximum - origin) / direction;

        if (first > second)
            (first, second) = (second, first);

        enterDistance = Mathf.Max(enterDistance, first);
        exitDistance = Mathf.Min(exitDistance, second);
        return enterDistance <= exitDistance + Epsilon;
    }
}
