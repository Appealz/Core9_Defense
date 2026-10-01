using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
public class GameManager : IUpdatable
{
    private readonly GameStateManager _gameStateManager;
    private readonly WaveManager _waveManager;
    private readonly IReadOnlyList<WaveData> _waveDataList;

    private int _currentWaveNumber = 1;

    public int CurrentWaveNumber => _currentWaveNumber;

    public GameManager(GameStateManager gameStateManager, WaveManager waveManager, IReadOnlyList<WaveData> waveDataList)
    {
        _gameStateManager = gameStateManager;
        _waveManager = waveManager;
        _waveDataList = waveDataList;
    }

    public void StartGame()
    {
        _gameStateManager.ChangeState(GameState.Playing);
        PlayCurrentWaveAsync().Forget();
    }

    public void StartNextWave()
    {
        _currentWaveNumber++;

        _gameStateManager.ChangeState(GameState.Playing);
        PlayCurrentWaveAsync().Forget();
    }

    public void Update(float deltaTime)
    {
        if (_gameStateManager.CurrentState != GameState.Playing)
            return;

        UpdatePlaying(deltaTime);
    }

    private void UpdatePlaying(float deltaTime)
    {
    }

    private async UniTask PlayCurrentWaveAsync()
    {
        if (_waveDataList.Count == 0)
            return;

        int waveDataIndex = (_currentWaveNumber - 1) % _waveDataList.Count;
        WaveData waveData = _waveDataList[waveDataIndex];

        await _waveManager.PlayWaveAsync(waveData);

        _gameStateManager.ChangeState(GameState.WaveClear);
        Debug.Log($"Wave {_currentWaveNumber} Clear");
    }
}