using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class GameManager : IUpdatable
{
    private readonly GameStateManager _gameStateManager;
    private readonly WaveManager _waveManager;
    private readonly RewardSelectionManager _rewardSelectionManager;
    private readonly IReadOnlyList<WaveData> _waveDataList;

    private int _currentWaveNumber = 1;

    public int CurrentWaveNumber => _currentWaveNumber;

    public GameManager(GameStateManager gameStateManager, WaveManager waveManager, RewardSelectionManager rewardSelectionManager, IReadOnlyList<WaveData> waveDataList)
    {
        _gameStateManager = gameStateManager;
        _waveManager = waveManager;
        _rewardSelectionManager = rewardSelectionManager;
        _waveDataList = waveDataList;
    }

    public void StartGame()
    {
        _gameStateManager.ChangeState(GameState.Playing);
        PlayCurrentWaveAsync().Forget();
    }

    public void StartNextWave()
    {
        if (_gameStateManager.CurrentState == GameState.GameOver)
            return;

        _currentWaveNumber++;

        _gameStateManager.ChangeState(GameState.Playing);
        PlayCurrentWaveAsync().Forget();
    }

    public void GameOver()
    {
        if (_gameStateManager.CurrentState == GameState.GameOver)
            return;

        _gameStateManager.ChangeState(GameState.GameOver);
        Debug.Log("Game Over");
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

        if (_gameStateManager.CurrentState == GameState.GameOver)
            return;

        _gameStateManager.ChangeState(GameState.WaveClear);
        Debug.Log($"Wave {_currentWaveNumber} Clear");

        _rewardSelectionManager.CreateChoices(3);
        _gameStateManager.ChangeState(GameState.RewardSelection);

        if (_rewardSelectionManager.CurrentChoices.Count == 0)
        {
            Debug.LogWarning("적용 가능한 보상이 없습니다.");
            return;
        }

        RewardData rewardData = _rewardSelectionManager.CurrentChoices[0];
        bool isApplied = await _rewardSelectionManager.ApplyAsync(rewardData);

        Debug.Log($"Reward Applied: {rewardData.RewardName}, Result: {isApplied}");
    }
}