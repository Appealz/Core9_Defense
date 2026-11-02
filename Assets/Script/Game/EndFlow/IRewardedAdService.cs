using Cysharp.Threading.Tasks;

public interface IRewardedAdService
{
    UniTask<bool> ShowAsync(string placementId);
}

public sealed class MockRewardedAdService : IRewardedAdService
{
    public UniTask<bool> ShowAsync(string placementId)
    {
        return UniTask.FromResult(true);
    }
}
