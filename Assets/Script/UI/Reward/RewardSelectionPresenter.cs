
using System;
using Cysharp.Threading.Tasks;

public class RewardSelectionPresenter : IDisposable
{
    private readonly RewardSelectionView _view;
    private readonly GameStateManager _gameStateManager;
    private readonly GameManager _gameManager;
    private readonly RewardSelectionManager _rewardSelectionManager;

    public RewardSelectionPresenter(RewardSelectionView view, GameStateManager gameStateManager, GameManager gameManager, RewardSelectionManager rewardSelectionManager)
    {
        _view = view;
        _gameStateManager = gameStateManager;
        _gameManager = gameManager;
        _rewardSelectionManager = rewardSelectionManager;

        _gameStateManager.StateChanged += OnStateChanged;
        _view.RewardSelected += OnRewardSelected;

        _view.Hide();
    }

    private void OnStateChanged(GameState previousState, GameState nextState)
    {
        if (nextState == GameState.RewardSelection)
            _view.Show(_rewardSelectionManager.CurrentChoices);
        else
            _view.Hide();
    }

    private void OnRewardSelected(RewardData rewardData)
    {
        _view.Hide();
        _gameManager.SelectRewardAsync(rewardData).Forget();
    }

    public void Dispose()
    {
        _gameStateManager.StateChanged -= OnStateChanged;
        _view.RewardSelected -= OnRewardSelected;
    }
}
