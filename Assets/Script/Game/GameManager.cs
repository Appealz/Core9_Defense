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
    private readonly PlayerProgression _playerProgression;

    private int _currentWaveNumber = 1;
    private float _rearrangementTimeRemaining;
    private bool _isRewardApplying;
    private int _rewardBatchTotal;
    private int _rewardBatchProcessed;

    public event Action<int> RearrangementTimeChanged;
    public event Action<IReadOnlyList<RewardData>, int, int> RewardChoicesChanged;

    public int CurrentWaveNumber => _currentWaveNumber;
    public int RearrangementRemainingSeconds => Mathf.CeilToInt(_rearrangementTimeRemaining);

    public GameManager(
        GameStateManager gameStateManager,
        WaveManager waveManager,
        RewardSelectionManager rewardSelectionManager,
        IReadOnlyList<WaveData> waveDataList,
        PlayerProgression playerProgression)
    {
        _gameStateManager = gameStateManager;
        _waveManager = waveManager;
        _rewardSelectionManager = rewardSelectionManager;
        _waveDataList = waveDataList;
        _playerProgression = playerProgression ?? throw new ArgumentNullException(nameof(playerProgression));
    }

    public void StartGame()
    {
        _playerProgression.Reset();
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
            bool isApplied = await _rewardSelectionManager.ApplyAsync(rewardData);

            if (!isApplied)
            {
                PublishCurrentChoices();
                return;
            }

            _playerProgression.TryConsumeReward();
            _rewardBatchProcessed++;

            if (_gameStateManager.CurrentState == GameState.GameOver)
                return;

            if (_playerProgression.PendingRewardCount > 0)
                ShowNextRewardSelection();
            else
                BeginRearrangement();
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);

            if (_gameStateManager.CurrentState != GameState.GameOver)
                PublishCurrentChoices();
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
        if (_gameStateManager.CurrentState == GameState.Rearrangement)
            UpdateRearrangement(deltaTime);
    }

    private void UpdateRearrangement(float deltaTime)
    {
        int previousSeconds = RearrangementRemainingSeconds;
        _rearrangementTimeRemaining = Mathf.Max(0f, _rearrangementTimeRemaining - deltaTime);
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
        await _waveManager.PlayWaveAsync(_waveDataList[waveDataIndex]);

        if (_gameStateManager.CurrentState == GameState.GameOver)
            return;

        _gameStateManager.ChangeState(GameState.WaveClear);
        Debug.Log($"Wave {_currentWaveNumber} Clear");

        if (_playerProgression.PendingRewardCount <= 0)
        {
            BeginRearrangement();
            return;
        }

        _rewardBatchTotal = _playerProgression.PendingRewardCount;
        _rewardBatchProcessed = 0;
        ShowNextRewardSelection();
    }

    private void ShowNextRewardSelection()
    {
        IReadOnlyList<RewardData> choices = _rewardSelectionManager.CreateChoices();

        if (choices.Count == 0)
        {
            Debug.LogWarning("선택 가능한 보상이 없어 선택권을 보존하고 재배치 단계로 진행합니다.");
            BeginRearrangement();
            return;
        }

        if (_gameStateManager.CurrentState != GameState.RewardSelection)
            _gameStateManager.ChangeState(GameState.RewardSelection);

        RewardChoicesChanged?.Invoke(choices, _rewardBatchProcessed + 1, _rewardBatchTotal);
    }

    private void PublishCurrentChoices()
    {
        if (_gameStateManager.CurrentState != GameState.RewardSelection)
            _gameStateManager.ChangeState(GameState.RewardSelection);

        RewardChoicesChanged?.Invoke(
            _rewardSelectionManager.CurrentChoices,
            _rewardBatchProcessed + 1,
            _rewardBatchTotal);
    }

    private void BeginRearrangement()
    {
        _rearrangementTimeRemaining = RearrangementDuration;
        _gameStateManager.ChangeState(GameState.Rearrangement);
    }
}
