using System;
using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;

public class AddressableProvider
{
    public async UniTask<T> LoadAssetAsync<T>(string key) where T : UnityEngine.Object
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Addressable Key가 비어 있습니다.", nameof(key));

        T asset = await Addressables.LoadAssetAsync<T>(key).Task;

        if (asset == null)
            throw new InvalidOperationException($"{key} 에셋을 불러오지 못했습니다.");

        return asset;
    }

    public void ReleaseAsset<T>(T asset) where T : UnityEngine.Object
    {
        if (asset == null)
            return;

        Addressables.Release(asset);
    }
}

public static class AddressableKeys
{
    public const string EnemyPrefab = "Enemy";
    public const string TowerPrefab = "Tower";
    public const string ProjectilePrefab = "Projectile";
}