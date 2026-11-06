using System;

public sealed class LobbyPresenter : IDisposable
{
    private readonly LobbyView _view;
    private readonly SceneTransition _sceneTransition;

    public event Action UpgradeRequested;
    public event Action OptionsRequested;

    public LobbyPresenter(LobbyView view, SceneTransition sceneTransition)
    {
        _view = view ?? throw new ArgumentNullException(nameof(view));
        _sceneTransition = sceneTransition ?? throw new ArgumentNullException(nameof(sceneTransition));
        _view.Initialize();
        _view.PlayClicked += HandlePlayClicked;
        _view.UpgradeClicked += HandleUpgradeClicked;
        _view.OptionsClicked += HandleOptionsClicked;
    }

    public void Show() => _view.SetVisible(true);
    public void Hide() => _view.SetVisible(false);

    private void HandlePlayClicked()
    {
        _sceneTransition.TryLoadScene(GameEndFlowManager.BattleSceneName);
    }

    private void HandleUpgradeClicked() => UpgradeRequested?.Invoke();
    private void HandleOptionsClicked() => OptionsRequested?.Invoke();

    public void Dispose()
    {
        _view.PlayClicked -= HandlePlayClicked;
        _view.UpgradeClicked -= HandleUpgradeClicked;
        _view.OptionsClicked -= HandleOptionsClicked;
    }
}
