using Cysharp.Threading.Tasks;

public interface IReward
{
    bool CanApply();
    UniTask<bool> ApplyAsync();
}