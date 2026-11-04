using System;
using UnityEngine;
using UnityEngine.UI;

public sealed class BattleSettingsView : MonoBehaviour
{
    [SerializeField] private Button _settingsButton;
    [SerializeField] private GameObject _overlayRoot;
    [SerializeField] private Slider _bgmSlider;
    [SerializeField] private Slider _sfxSlider;
    [SerializeField] private Button _returnToLobbyButton;
    [SerializeField] private Button _restartButton;
    [SerializeField] private Button _resumeButton;

    private bool _isInitialized;

    public event Action SettingsClicked;
    public event Action ReturnToLobbyClicked;
    public event Action RestartClicked;
    public event Action ResumeClicked;

    public bool IsConfigured =>
        _settingsButton != null && _overlayRoot != null &&
        _bgmSlider != null && _sfxSlider != null &&
        _returnToLobbyButton != null && _restartButton != null &&
        _resumeButton != null;

    public bool IsOpen => _overlayRoot != null && _overlayRoot.activeSelf;

    public void Initialize()
    {
        if (_isInitialized || !IsConfigured)
            return;

        _settingsButton.onClick.AddListener(HandleSettingsClicked);
        _returnToLobbyButton.onClick.AddListener(HandleReturnToLobbyClicked);
        _restartButton.onClick.AddListener(HandleRestartClicked);
        _resumeButton.onClick.AddListener(HandleResumeClicked);

        _bgmSlider.interactable = false;
        _sfxSlider.interactable = false;
        _overlayRoot.SetActive(false);
        _isInitialized = true;
    }

    public void SetOpen(bool isOpen)
    {
        if (_overlayRoot != null && _overlayRoot.activeSelf != isOpen)
            _overlayRoot.SetActive(isOpen);
    }

    public void SetSettingsButtonInteractable(bool isInteractable)
    {
        if (_settingsButton != null)
            _settingsButton.interactable = isInteractable;
    }

    private void HandleSettingsClicked() => SettingsClicked?.Invoke();
    private void HandleReturnToLobbyClicked() => ReturnToLobbyClicked?.Invoke();
    private void HandleRestartClicked() => RestartClicked?.Invoke();
    private void HandleResumeClicked() => ResumeClicked?.Invoke();

    private void OnDestroy()
    {
        if (!_isInitialized)
            return;

        if (_settingsButton != null)
            _settingsButton.onClick.RemoveListener(HandleSettingsClicked);

        if (_returnToLobbyButton != null)
            _returnToLobbyButton.onClick.RemoveListener(HandleReturnToLobbyClicked);

        if (_restartButton != null)
            _restartButton.onClick.RemoveListener(HandleRestartClicked);

        if (_resumeButton != null)
            _resumeButton.onClick.RemoveListener(HandleResumeClicked);
    }
}
