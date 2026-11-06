public interface IGameSpeedAccess
{
    bool CanUseTripleSpeed { get; }
}

public sealed class GameSpeedAccess : IGameSpeedAccess
{
    private readonly bool _hasRemoveAdsEntitlement;

    public GameSpeedAccess(bool hasRemoveAdsEntitlement)
    {
        _hasRemoveAdsEntitlement = hasRemoveAdsEntitlement;
    }

    public bool CanUseTripleSpeed
    {
        get
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            return true;
#else
            return _hasRemoveAdsEntitlement;
#endif
        }
    }
}
