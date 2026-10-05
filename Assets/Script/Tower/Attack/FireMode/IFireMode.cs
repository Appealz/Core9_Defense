using UnityEngine;

public interface IFireMode
{
    bool IsFiring { get; }

    void Start();
    void Update(float deltaTime);
    bool TryGetShot(out FireShot shot);
}

public readonly struct FireShot
{
    public float AngleOffset { get; }

    public FireShot(float angleOffset)
    {
        AngleOffset = angleOffset;
    }
}