
using Cysharp.Threading.Tasks;
using UnityEngine;

public class SpawnManager
{
    private readonly EnemyFactory _enemyFactory;
    private readonly EnemyManager _enemyManager;
    private readonly EnemySpawnPositionProvider _spawnPositionProvider;
    private readonly Transform _moveTarget;

    public Vector3 CorePosition => _moveTarget.position;

    public SpawnManager(EnemyFactory enemyFactory, EnemyManager enemyManager, EnemySpawnPositionProvider spawnPositionProvider, Transform moveTarget)
    {
        _enemyFactory = enemyFactory;
        _enemyManager = enemyManager;
        _spawnPositionProvider = spawnPositionProvider;
        _moveTarget = moveTarget;
    }

    public UniTask<Enemy> SpawnEnemyAsync(EnemyConfig config)
    {
        return SpawnEnemyAsync(config, _spawnPositionProvider.GetPosition());
    }

    public UniTask<Enemy> SpawnEnemyAsync(EnemyConfig config, Vector3 position)
    {
        return SpawnEnemyAsync(config, position, position, 0f);
    }

    public async UniTask<Enemy> SpawnEnemyAsync(EnemyConfig config, Vector3 position, Vector3 targetPosition, float duration)
    {
        Enemy enemy = await _enemyFactory.CreateAsync(config, _moveTarget);

        enemy.transform.position = position;

        if (duration > 0f)
            enemy.Visual.PlaySpawnSpread(targetPosition, duration);

        _enemyManager.Register(enemy);
        enemy.gameObject.SetActive(true);

        return enemy;
    }
}
