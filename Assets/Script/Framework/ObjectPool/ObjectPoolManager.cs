using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class ObjectPoolManager
{
    private readonly AddressableProvider _addressableProvider;
    private readonly Dictionary<Type, object> _pools = new();

    public ObjectPoolManager(AddressableProvider addressableProvider)
    {
        _addressableProvider = addressableProvider ?? throw new ArgumentNullException(nameof(addressableProvider));
    }

    public async UniTask InitializeAsync()
    {
        await CreatePoolAsync<Enemy>(AddressableKeys.EnemyPrefab, 10, onReturn: enemy => enemy.gameObject.SetActive(false));
        await CreatePoolAsync<Projectile>(AddressableKeys.ProjectilePrefab, 30, onReturn: projectile => projectile.gameObject.SetActive(false));
    }

    private async UniTask CreatePoolAsync<T>(string key, int prewarmCount, Action<T> onGet = null, Action<T> onReturn = null) where T : Component
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Addressable Key가 비어 있습니다.", nameof(key));

        if (_pools.ContainsKey(typeof(T)))
            throw new InvalidOperationException($"{typeof(T).Name} Pool이 이미 생성되어 있습니다.");

        GameObject prefab = await _addressableProvider.LoadAssetAsync<GameObject>(key);

        var pool = new ObjectPool<T>(() => CreateInstanceAsync<T>(prefab), onGet, onReturn);

        await pool.PrewarmAsync(prewarmCount);

        _pools.Add(typeof(T), pool);
    }

    public ObjectPool<T> GetPool<T>() where T : Component
    {
        if (!_pools.TryGetValue(typeof(T), out object pool))
            throw new InvalidOperationException($"{typeof(T).Name} Pool이 생성되어 있지 않습니다.");

        return (ObjectPool<T>)pool;
    }

    private UniTask<T> CreateInstanceAsync<T>(GameObject prefab) where T : Component
    {
        GameObject instance = UnityEngine.Object.Instantiate(prefab);
        T component = instance.GetComponent<T>();

        if (component == null)
        {
            UnityEngine.Object.Destroy(instance);
            throw new InvalidOperationException($"{typeof(T).Name} 컴포넌트가 Prefab에 없습니다.");
        }

        return UniTask.FromResult(component);
    }
}