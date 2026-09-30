using UnityEngine;

public sealed class CameraBoundsProvider
{
    private readonly Camera _camera;

    public CameraBoundsProvider(Camera camera)
    {
        _camera = camera;
    }

    public CameraBounds GetBounds()
    {
        Vector3 bottomLeft =
            _camera.ViewportToWorldPoint(new Vector3(0f, 0f, 0f));

        Vector3 topRight =
            _camera.ViewportToWorldPoint(new Vector3(1f, 1f, 0f));

        return new CameraBounds(
            bottomLeft.x,
            topRight.x,
            bottomLeft.y,
            topRight.y);
    }
}

public readonly struct CameraBounds
{
    public float Left { get; }
    public float Right { get; }
    public float Bottom { get; }
    public float Top { get; }

    public CameraBounds(
        float left,
        float right,
        float bottom,
        float top)
    {
        Left = left;
        Right = right;
        Bottom = bottom;
        Top = top;
    }
}