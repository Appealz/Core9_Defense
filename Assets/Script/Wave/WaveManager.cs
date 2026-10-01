using System;
using System.Threading;
using Cysharp.Threading.Tasks;

public class WaveManager
{
    private readonly SpawnManager _spawnManager;
    private readonly EnemyManager _enemyManager;

    public WaveManager(SpawnManager spawnManager, EnemyManager enemyManager)
    {
        _spawnManager = spawnManager;
        _enemyManager = enemyManager;
    }

    public async UniTask PlayWaveAsync(WaveData waveData, CancellationToken cancellationToken = default)
    {
        foreach (WaveSpawnEntry entry in waveData.SpawnEntries)
        {
            await SpawnEntryAsync(entry, cancellationToken);
        }

        await UniTask.WaitUntil(() => _enemyManager.AliveCount == 0, cancellationToken: cancellationToken);
    }

    private async UniTask SpawnEntryAsync(WaveSpawnEntry entry, CancellationToken cancellationToken)
    {
        for (int i = 0; i < entry.SpawnCount; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            await _spawnManager.SpawnEnemyAsync(entry.EnemyConfig);

            bool isLastSpawn = i == entry.SpawnCount - 1;

            if (isLastSpawn)
                continue;

            await UniTask.Delay(TimeSpan.FromSeconds(entry.SpawnInterval), cancellationToken: cancellationToken);
        }
    }
}