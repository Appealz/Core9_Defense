
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class SpawnManager
{
    private readonly EnemyFactory _enemyFactory;
    private readonly EnemyManager _enemyManager;
    private readonly EnemySpawnPositionProvider _spawnPositionProvider;
    private readonly Transform _moveTarget;

    private CancellationToken _waveCancellationToken;
    private EnemyRuntimeModifiers _waveRuntimeModifiers = EnemyRuntimeModifiers.Default;
    private int _waveVersion;

    public Vector3 CorePosition => _moveTarget.position;
    internal CancellationToken WaveCancellationToken => _waveCancellationToken;

    public SpawnManager(EnemyFactory enemyFactory, EnemyManager enemyManager, EnemySpawnPositionProvider spawnPositionProvider, Transform moveTarget)
    {
        _enemyFactory = enemyFactory;
        _enemyManager = enemyManager;
        _spawnPositionProvider = spawnPositionProvider;
        _moveTarget = moveTarget;
    }

    public UniTask<Enemy> SpawnEnemyAsync(EnemyConfig config)
    {
        return SpawnEnemyAsync(
            config,
            _spawnPositionProvider.GetPosition(),
            _waveRuntimeModifiers,
            _waveCancellationToken);
    }

    public UniTask<Enemy> SpawnEnemyAsync(EnemyConfig config, Vector3 position)
    {
        return SpawnEnemyAsync(
            config,
            position,
            _waveRuntimeModifiers,
            _waveCancellationToken);
    }

    public async UniTask<Enemy> SpawnEnemyAsync(EnemyConfig config, Vector3 position, Vector3 targetPosition, float duration)
    {
        return await SpawnEnemyAsync(
            config,
            position,
            targetPosition,
            duration,
            _waveRuntimeModifiers,
            _waveCancellationToken);
    }

    public UniTask<Enemy> SpawnEnemyAsync(
        EnemyConfig config,
        EnemyRuntimeModifiers runtimeModifiers,
        CancellationToken cancellationToken)
    {
        return SpawnEnemyAsync(
            config,
            _spawnPositionProvider.GetPosition(),
            runtimeModifiers,
            cancellationToken);
    }

    public UniTask<Enemy> SpawnEnemyAsync(
        EnemyConfig config,
        Vector3 position,
        EnemyRuntimeModifiers runtimeModifiers,
        CancellationToken cancellationToken)
    {
        return SpawnEnemyAsync(
            config,
            position,
            position,
            0f,
            runtimeModifiers,
            cancellationToken);
    }

    public async UniTask<Enemy> SpawnEnemyAsync(
        EnemyConfig config,
        Vector3 position,
        Vector3 targetPosition,
        float duration,
        EnemyRuntimeModifiers runtimeModifiers,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Enemy enemy = await _enemyFactory.CreateAsync(
            config,
            _moveTarget,
            runtimeModifiers,
            cancellationToken);

        if (cancellationToken.IsCancellationRequested)
        {
            _enemyFactory.Release(enemy);
            cancellationToken.ThrowIfCancellationRequested();
        }

        enemy.transform.position = position;

        if (duration > 0f)
            enemy.Visual.PlaySpawnSpread(targetPosition, duration);

        _enemyManager.Register(enemy);
        enemy.gameObject.SetActive(true);

        return enemy;
    }

    public int BeginWave(
        EnemyRuntimeModifiers runtimeModifiers,
        CancellationToken cancellationToken)
    {
        _waveRuntimeModifiers = runtimeModifiers;
        _waveCancellationToken = cancellationToken;
        _waveVersion = unchecked(_waveVersion + 1);
        return _waveVersion;
    }

    public void EndWave(int waveVersion)
    {
        if (_waveVersion != waveVersion)
            return;

        _waveCancellationToken = CancellationToken.None;
        _waveRuntimeModifiers = EnemyRuntimeModifiers.Default;
    }
}
