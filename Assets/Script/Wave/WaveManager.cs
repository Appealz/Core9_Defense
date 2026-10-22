
using System;
using System.Threading;
using Cysharp.Threading.Tasks;

public class WaveManager
{
    public event Action<int, int> ProgressChanged;

    private readonly SpawnManager _spawnManager;
    private readonly EnemyManager _enemyManager;

    private int _totalEnemyCount;
    private int _processedEnemyCount;

    public WaveManager(SpawnManager spawnManager, EnemyManager enemyManager)
    {
        _spawnManager = spawnManager;
        _enemyManager = enemyManager;
    }

    public async UniTask PlayWaveAsync(WaveData waveData, CancellationToken cancellationToken = default)
    {
        _totalEnemyCount = CalculateTotalEnemyCount(waveData);
        _processedEnemyCount = 0;

        ProgressChanged?.Invoke(_processedEnemyCount, _totalEnemyCount);

        _enemyManager.EnemyRemoved += HandleEnemyRemoved;

        try
        {
            foreach (WaveSpawnEntry entry in waveData.SpawnEntries)
            {
                await SpawnEntryAsync(entry, cancellationToken);
            }

            await UniTask.WaitUntil(
                () => _enemyManager.AliveCount == 0,
                cancellationToken: cancellationToken
            );
        }
        finally
        {
            _enemyManager.EnemyRemoved -= HandleEnemyRemoved;
        }
    }

    private int CalculateTotalEnemyCount(WaveData waveData)
    {
        int totalCount = 0;

        foreach (WaveSpawnEntry entry in waveData.SpawnEntries)
            totalCount += Math.Max(0, entry.SpawnCount);

        return totalCount;
    }

    private void HandleEnemyRemoved()
    {
        _processedEnemyCount = Math.Min(
            _processedEnemyCount + 1,
            _totalEnemyCount
        );

        ProgressChanged?.Invoke(_processedEnemyCount, _totalEnemyCount);
    }

    private async UniTask SpawnEntryAsync(
        WaveSpawnEntry entry,
        CancellationToken cancellationToken)
    {
        for (int i = 0; i < entry.SpawnCount; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            await _spawnManager.SpawnEnemyAsync(entry.EnemyConfig);

            bool isLastSpawn = i == entry.SpawnCount - 1;

            if (isLastSpawn)
                continue;

            await UniTask.Delay(
                TimeSpan.FromSeconds(entry.SpawnInterval),
                cancellationToken: cancellationToken
            );
        }
    }
}
