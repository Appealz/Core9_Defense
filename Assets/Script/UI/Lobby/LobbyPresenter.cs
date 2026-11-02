using System;
using UnityEngine.SceneManagement;

public sealed class LobbyPresenter : IDisposable
{
    private readonly LobbyView _view;

    public event Action UpgradeRequested;

    public LobbyPresenter(LobbyView view)
    {
        _view = view ?? throw new ArgumentNullException(nameof(view));
        _view.Initialize();
        _view.PlayClicked += HandlePlayClicked;
        _view.UpgradeClicked += HandleUpgradeClicked;
    }

    public void Show() => _view.SetVisible(true);
    public void Hide() => _view.SetVisible(false);

    private static void HandlePlayClicked()
    {
        SceneManager.LoadScene(GameEndFlowManager.BattleSceneName, LoadSceneMode.Single);
    }

    private void HandleUpgradeClicked() => UpgradeRequested?.Invoke();

    public void Dispose()
    {
        _view.PlayClicked -= HandlePlayClicked;
        _view.UpgradeClicked -= HandleUpgradeClicked;
    }
}
