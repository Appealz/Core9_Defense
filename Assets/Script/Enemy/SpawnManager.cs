using Cysharp.Threading.Tasks;
using UnityEngine;

public class SpawnManager
{
    private readonly EnemyFactory _enemyFactory;
    private readonly EnemySpawnPositionProvider _spawnPositionProvider;
    private readonly Transform _moveTarget;

    public SpawnManager(EnemyFactory enemyFactory, EnemySpawnPositionProvider spawnPositionProvider, Transform moveTarget)
    {
        _enemyFactory = enemyFactory;
        _spawnPositionProvider = spawnPositionProvider;
        _moveTarget = moveTarget;
    }

    public async UniTask<Enemy> SpawnEnemyAsync(EnemyConfig config)
    {
        Enemy enemy = await _enemyFactory.CreateAsync(config, _moveTarget);

        Vector2 spawnPosition = _spawnPositionProvider.GetPosition();
        enemy.transform.position = spawnPosition;

        return enemy;
    }
}