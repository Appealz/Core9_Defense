
using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class GameManager : IUpdatable
{
    private const float RearrangementDuration = 30f;

    private readonly GameStateManager _gameStateManager;
    private readonly WaveManager _waveManager;
    private readonly RewardSelectionManager _rewardSelectionManager;
    private readonly IReadOnlyList<WaveData> _waveDataList;

    private int _currentWaveNumber = 1;
    private float _rearrangementTimeRemaining;
    private bool _isRewardApplying;

    public event Action<int> RearrangementTimeChanged;

    public int CurrentWaveNumber => _currentWaveNumber;
    public int RearrangementRemainingSeconds => Mathf.CeilToInt(_rearrangementTimeRemaining);

    public GameManager(
        GameStateManager gameStateManager,
        WaveManager waveManager,
        RewardSelectionManager rewardSelectionManager,
        IReadOnlyList<WaveData> waveDataList)
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

        _rearrangementTimeRemaining = 0f;
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
            {
                _rearrangementTimeRemaining = RearrangementDuration;
                _gameStateManager.ChangeState(GameState.Rearrangement);
            }
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
        switch (_gameStateManager.CurrentState)
        {
            case GameState.Playing:
                UpdatePlaying(deltaTime);
                break;

            case GameState.Rearrangement:
                UpdateRearrangement(deltaTime);
                break;
        }
    }

    private void UpdatePlaying(float deltaTime)
    {
    }

    private void UpdateRearrangement(float deltaTime)
    {
        int previousSeconds = RearrangementRemainingSeconds;

        _rearrangementTimeRemaining = Mathf.Max(
            0f,
            _rearrangementTimeRemaining - deltaTime
        );

        int remainingSeconds = RearrangementRemainingSeconds;

        if (previousSeconds != remainingSeconds)
            RearrangementTimeChanged?.Invoke(remainingSeconds);

        if (_rearrangementTimeRemaining <= 0f)
            StartNextWave();
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

        if (_rewardSelectionManager.CreateChoices().Count == 0)
        {
            Debug.LogWarning("선택 가능한 보상이 없어 보상 선택 단계를 건너뜁니다. 보상 데이터를 확인하세요.");
            BeginRearrangement();
            return;
        }

        _gameStateManager.ChangeState(GameState.RewardSelection);
    }

    private void BeginRearrangement()
    {
        _rearrangementTimeRemaining = RearrangementDuration;
        _gameStateManager.ChangeState(GameState.Rearrangement);
    }
}
