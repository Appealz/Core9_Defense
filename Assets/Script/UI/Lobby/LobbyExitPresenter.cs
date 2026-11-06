using System;
using UnityEngine;

public sealed class LobbyExitPresenter : IDisposable
{
    private readonly ExitConfirmationView _view;
    private readonly LobbyBackInput _backInput;
    private readonly SceneTransition _sceneTransition;
    private readonly Func<bool> _tryCloseTopmostLobbyContent;
    private readonly Action _quitApplication;

    private bool _isExitRequested;

    public LobbyExitPresenter(
        ExitConfirmationView view,
        LobbyBackInput backInput,
        SceneTransition sceneTransition,
        Func<bool> tryCloseTopmostLobbyContent = null,
        Action quitApplication = null)
    {
        _view = view ?? throw new ArgumentNullException(nameof(view));
        _backInput = backInput ?? throw new ArgumentNullException(nameof(backInput));
        _sceneTransition = sceneTransition ?? throw new ArgumentNullException(nameof(sceneTransition));
        _tryCloseTopmostLobbyContent = tryCloseTopmostLobbyContent;
        _quitApplication = quitApplication ?? QuitApplication;

        _view.Initialize();
        _backInput.Initialize();
        _view.CancelClicked += HandleCancelClicked;
        _view.ExitClicked += HandleExitClicked;
        _backInput.BackRequested += HandleBackRequested;
        _view.SetButtonsInteractable(true);
        _view.SetVisible(false);
    }

    public void HandleBackRequested()
    {
        if (_isExitRequested || _sceneTransition.IsTransitioning)
            return;

        if (_view.IsVisible)
        {
            _view.SetVisible(false);
            return;
        }

        if (_tryCloseTopmostLobbyContent?.Invoke() == true)
            return;

        _view.SetVisible(true);
    }

    private void HandleCancelClicked()
    {
        if (_isExitRequested)
            return;

        _view.SetVisible(false);
    }

    private void HandleExitClicked()
    {
        if (_isExitRequested || !_view.IsVisible)
            return;

        _isExitRequested = true;
        _view.SetButtonsInteractable(false);
        _quitApplication();
    }

    private static void QuitApplication()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void Dispose()
    {
        _view.CancelClicked -= HandleCancelClicked;
        _view.ExitClicked -= HandleExitClicked;
        _backInput.BackRequested -= HandleBackRequested;
    }
}
