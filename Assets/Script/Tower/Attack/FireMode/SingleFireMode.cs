public sealed class SingleFireMode : IFireMode
{
    private bool _canFire;

    public bool IsFiring => _canFire;

    public void Start()
    {
        _canFire = true;
    }

    public void Update(float deltaTime)
    {
    }

    public bool TryGetShot(out FireShot shot)
    {
        shot = default;

        if (!_canFire)
            return false;

        _canFire = false;
        shot = new FireShot(0f);

        return true;
    }
}