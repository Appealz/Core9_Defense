using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class ObjectPoolManager
{
    private readonly AddressableProvider _addressableProvider;
    private readonly Transform _objectPoolsRoot;
    private readonly Transform _combatTextRoot;
    private readonly Dictionary<Type, object> _pools = new();

    public ObjectPoolManager(
        AddressableProvider addressableProvider,
        Transform objectPoolsRoot,
        Transform combatTextRoot)
    {
        _addressableProvider = addressableProvider ?? throw new ArgumentNullException(nameof(addressableProvider));
        _objectPoolsRoot = objectPoolsRoot ?? throw new ArgumentNullException(nameof(objectPoolsRoot));
        _combatTextRoot = combatTextRoot ?? throw new ArgumentNullException(nameof(combatTextRoot));
    }

    public async UniTask InitializeAsync()
    {
        Transform enemiesRoot = GetPoolRoot("Enemies");
        Transform projectilesRoot = GetPoolRoot("Projectiles");
        Transform explosionsRoot = GetPoolRoot("Explosions");
        Transform lasersRoot = GetPoolRoot("Lasers");

        await CreatePoolAsync<Enemy>(AddressableKeys.EnemyPrefab, 10, enemiesRoot, onReturn: enemy => enemy.gameObject.SetActive(false));
        await CreatePoolAsync<Projectile>(AddressableKeys.ProjectilePrefab, 30, projectilesRoot, onReturn: projectile => projectile.gameObject.SetActive(false));
        await CreatePoolAsync<ExplosionEffect>(AddressableKeys.ExplosionEffectPrefab, 10, explosionsRoot, onReturn: effect => effect.gameObject.SetActive(false));
        await CreatePoolAsync<Laser>(AddressableKeys.LaserPrefab, 10, lasersRoot, onReturn: laser => laser.gameObject.SetActive(false));
        await CreatePoolAsync<FloatingCombatText>(AddressableKeys.FloatingCombatTextPrefab, 30, _combatTextRoot, onReturn: text => text.gameObject.SetActive(false));
    }

    private async UniTask CreatePoolAsync<T>(string key, int prewarmCount, Transform parent, Action<T> onGet = null, Action<T> onReturn = null) where T : Component
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Addressable Key가 비어 있습니다.", nameof(key));

        if (_pools.ContainsKey(typeof(T)))
            throw new InvalidOperationException($"{typeof(T).Name} Pool이 이미 생성되어 있습니다.");

        GameObject prefab = await _addressableProvider.LoadAssetAsync<GameObject>(key);

        var pool = new ObjectPool<T>(() => CreateInstanceAsync<T>(prefab, parent), onGet, onReturn);

        await pool.PrewarmAsync(prewarmCount);

        _pools.Add(typeof(T), pool);
    }

    public ObjectPool<T> GetPool<T>() where T : Component
    {
        if (!_pools.TryGetValue(typeof(T), out object pool))
            throw new InvalidOperationException($"{typeof(T).Name} Pool이 생성되어 있지 않습니다.");

        return (ObjectPool<T>)pool;
    }

    private Transform GetPoolRoot(string name)
    {
        Transform root = _objectPoolsRoot.Find(name);

        if (root == null)
            throw new InvalidOperationException($"{name} Pool 부모가 ObjectPools 아래에 없습니다.");

        return root;
    }

    private UniTask<T> CreateInstanceAsync<T>(GameObject prefab, Transform parent) where T : Component
    {
        GameObject instance = UnityEngine.Object.Instantiate(prefab, parent, false);
        T component = instance.GetComponent<T>();

        if (component == null)
        {
            UnityEngine.Object.Destroy(instance);
            throw new InvalidOperationException($"{typeof(T).Name} 컴포넌트가 Prefab에 없습니다.");
        }

        return UniTask.FromResult(component);
    }
}