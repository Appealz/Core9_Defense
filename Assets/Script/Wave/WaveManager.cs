
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

    public async UniTask PlayWaveAsync(WavePlan wavePlan, CancellationToken cancellationToken = default)
    {
        if (wavePlan == null)
            throw new ArgumentNullException(nameof(wavePlan));

        _totalEnemyCount = wavePlan.TotalEnemyCount;
        _processedEnemyCount = 0;
        _enemyManager.SetSupportRetreatWaiting(false);
        int waveVersion = _spawnManager.BeginWave(wavePlan.RuntimeModifiers, cancellationToken);

        ProgressChanged?.Invoke(_processedEnemyCount, _totalEnemyCount);

        _enemyManager.EnemyRemoved += HandleEnemyRemoved;
        _enemyManager.SpawnCountAdjusted += HandleSpawnCountAdjusted;

        try
        {
            await SpawnPlanAsync(wavePlan, cancellationToken);

            await UniTask.WaitUntil(
                TryCompleteWaveOrBeginSupportRetreat,
                cancellationToken: cancellationToken
            );
        }
        finally
        {
            _spawnManager.EndWave(waveVersion);
            _enemyManager.SetSupportRetreatWaiting(false);
            _enemyManager.EnemyRemoved -= HandleEnemyRemoved;
            _enemyManager.SpawnCountAdjusted -= HandleSpawnCountAdjusted;
        }
    }

    private bool TryCompleteWaveOrBeginSupportRetreat()
    {
        bool canWaitForRetreat = _enemyManager.PendingSpawnCount == 0 &&
            _enemyManager.CombatAliveCount == 0 &&
            _enemyManager.SupportAliveCount > 0;

        _enemyManager.SetSupportRetreatWaiting(canWaitForRetreat);

        if (_enemyManager.PendingSpawnCount > 0)
            return false;

        if (_enemyManager.AliveCount == 0)
            return true;

        if (_enemyManager.CombatAliveCount > 0)
            return false;

        return false;
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

    private async UniTask SpawnPlanAsync(WavePlan wavePlan, CancellationToken cancellationToken)
    {
        float previousSpawnTime = 0f;

        for (int i = 0; i < wavePlan.SpawnInstructions.Count; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            WaveSpawnInstruction instruction = wavePlan.SpawnInstructions[i];
            float delay = Math.Max(0f, instruction.SpawnTime - previousSpawnTime);

            if (delay > 0f)
            {
                await UniTask.Delay(
                    TimeSpan.FromSeconds(delay),
                    cancellationToken: cancellationToken);
            }

            await _spawnManager.SpawnEnemyAsync(
                instruction.EnemyConfig,
                wavePlan.RuntimeModifiers,
                cancellationToken);
            previousSpawnTime = instruction.SpawnTime;
        }
    }
}
