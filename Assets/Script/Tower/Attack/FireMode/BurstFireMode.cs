using System;

public sealed class BurstFireMode : IFireMode
{
    private readonly int _shotCount;
    private readonly float _shotInterval;

    private int _remainingShots;
    private float _shotTimer;

    public bool IsFiring => _remainingShots > 0;

    public BurstFireMode(int shotCount, float shotInterval)
    {
        if (shotCount <= 0)
            throw new ArgumentOutOfRangeException(nameof(shotCount));

        if (shotInterval < 0f)
            throw new ArgumentOutOfRangeException(nameof(shotInterval));

        _shotCount = shotCount;
        _shotInterval = shotInterval;
    }

    public void Start()
    {
        _remainingShots = _shotCount;
        _shotTimer = 0f;
    }

    public void Update(float deltaTime)
    {
        if (!IsFiring)
            return;

        _shotTimer -= deltaTime;
    }

    public bool TryGetShot(out FireShot shot)
    {
        shot = default;

        if (!IsFiring || _shotTimer > 0f)
            return false;

        _remainingShots--;

        if (_remainingShots > 0)
            _shotTimer = _shotInterval;

        shot = new FireShot(0f);

        return true;
    }
}