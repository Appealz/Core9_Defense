
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
    private bool _isRewardApplying;

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
        if (_gameStateManager.CurrentState != GameState.Rearrangement)
            return;

        _currentWaveNumber++;

        _gameStateManager.ChangeState(GameState.Playing);
        PlayCurrentWaveAsync().Forget();
    }

    public async UniTask SelectRewardAsync(RewardData rewardData)
    {
        if (_gameStateManager.CurrentState != GameState.RewardSelection || _isRewardApplying)
            return;

        _isRewardApplying = true;

        try
        {
            await _rewardSelectionManager.ApplyAsync(rewardData);

            if (_gameStateManager.CurrentState == GameState.RewardSelection)
                _gameStateManager.ChangeState(GameState.Rearrangement);
        }
        finally
        {
            _isRewardApplying = false;
        }
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

        if (_rewardSelectionManager.CreateChoices(3).Count == 0)
        {
            Debug.LogError("선택 가능한 보상이 없습니다. 보상 데이터를 확인하세요.");
            return;
        }

        _gameStateManager.ChangeState(GameState.RewardSelection);
    }
}
