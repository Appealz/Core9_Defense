using System;
using UnityEngine;
using UnityEngine.UI;

public sealed class ExitConfirmationView : MonoBehaviour
{
    [SerializeField] private GameObject _root;
    [SerializeField] private Button _cancelButton;
    [SerializeField] private Button _exitButton;

    private bool _isInitialized;

    public event Action CancelClicked;
    public event Action ExitClicked;

    public bool IsConfigured =>
        _root != null && _cancelButton != null && _exitButton != null;

    public bool IsVisible => _root != null && _root.activeSelf;

    public void Initialize()
    {
        if (_isInitialized || !IsConfigured)
            return;

        _cancelButton.onClick.AddListener(HandleCancelClicked);
        _exitButton.onClick.AddListener(HandleExitClicked);
        _isInitialized = true;
    }

    public void SetVisible(bool isVisible)
    {
        if (_root != null && _root.activeSelf != isVisible)
            _root.SetActive(isVisible);
    }

    public void SetButtonsInteractable(bool isInteractable)
    {
        if (_cancelButton != null)
            _cancelButton.interactable = isInteractable;

        if (_exitButton != null)
            _exitButton.interactable = isInteractable;
    }

    private void HandleCancelClicked() => CancelClicked?.Invoke();
    private void HandleExitClicked() => ExitClicked?.Invoke();

    private void OnDestroy()
    {
        if (!_isInitialized)
            return;

        _cancelButton.onClick.RemoveListener(HandleCancelClicked);
        _exitButton.onClick.RemoveListener(HandleExitClicked);
    }
}
