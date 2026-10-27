
using System;
using System.Collections.Generic;
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
        _enemyManager.SpawnCountAdjusted += HandleSpawnCountAdjusted;

        try
        {
            List<UniTask> spawnTasks = new();

            foreach (WaveSpawnEntry entry in waveData.SpawnEntries)
                spawnTasks.Add(SpawnEntryAsync(entry, cancellationToken));

            await UniTask.WhenAll(spawnTasks);

            await UniTask.WaitUntil(
                TryCompleteWaveOrBeginSupportRetreat,
                cancellationToken: cancellationToken
            );
        }
        finally
        {
            _enemyManager.EnemyRemoved -= HandleEnemyRemoved;
            _enemyManager.SpawnCountAdjusted -= HandleSpawnCountAdjusted;
        }
    }

    private bool TryCompleteWaveOrBeginSupportRetreat()
    {
        if (_enemyManager.PendingSpawnCount > 0)
            return false;

        if (_enemyManager.AliveCount == 0)
            return true;

        if (_enemyManager.CombatAliveCount > 0)
            return false;

        if (_enemyManager.SupportAliveCount > 0)
            _enemyManager.TryRetreatSupportEnemies();

        return false;
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
        _processedEnemyCount = Math.Min(_processedEnemyCount + 1, _totalEnemyCount);
        ProgressChanged?.Invoke(_processedEnemyCount, _totalEnemyCount);
    }

    private void HandleSpawnCountAdjusted(int count)
    {
        _totalEnemyCount = Math.Max(_processedEnemyCount, _totalEnemyCount + count);
        ProgressChanged?.Invoke(_processedEnemyCount, _totalEnemyCount);
    }

    private async UniTask SpawnEntryAsync(WaveSpawnEntry entry, CancellationToken cancellationToken)
    {
        if (entry.SpawnCount <= 0)
            return;

        if (entry.StartDelay > 0f)
            await UniTask.Delay(TimeSpan.FromSeconds(entry.StartDelay), cancellationToken: cancellationToken);

        for (int i = 0; i < entry.SpawnCount; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            await _spawnManager.SpawnEnemyAsync(entry.EnemyConfig);

            if (i < entry.SpawnCount - 1)
                await UniTask.Delay(TimeSpan.FromSeconds(Math.Max(0f, entry.SpawnInterval)), cancellationToken: cancellationToken);
        }
    }
}
