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
    public event Action<float> BgmVolumeChanged;
    public event Action<float> SfxVolumeChanged;

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
        ConfigureSlider(_bgmSlider);
        ConfigureSlider(_sfxSlider);
        _bgmSlider.onValueChanged.AddListener(HandleBgmVolumeChanged);
        _sfxSlider.onValueChanged.AddListener(HandleSfxVolumeChanged);

        _overlayRoot.SetActive(false);
        _isInitialized = true;
    }

    public void SetOpen(bool isOpen)
    {
        if (isOpen && _overlayRoot != null)
        {
            _overlayRoot.transform.SetAsLastSibling();

            if (_settingsButton != null)
                _settingsButton.transform.SetAsLastSibling();
        }

        if (_overlayRoot != null && _overlayRoot.activeSelf != isOpen)
            _overlayRoot.SetActive(isOpen);
    }

    public void SetSettingsButtonInteractable(bool isInteractable)
    {
        if (_settingsButton != null)
            _settingsButton.interactable = isInteractable;
    }

    public void SetAudioVolumes(float bgmVolume, float sfxVolume)
    {
        _bgmSlider.SetValueWithoutNotify(Mathf.Clamp01(bgmVolume));
        _sfxSlider.SetValueWithoutNotify(Mathf.Clamp01(sfxVolume));
    }

    private static void ConfigureSlider(Slider slider)
    {
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.wholeNumbers = false;
        slider.interactable = true;
    }

    private void HandleSettingsClicked() => SettingsClicked?.Invoke();
    private void HandleReturnToLobbyClicked() => ReturnToLobbyClicked?.Invoke();
    private void HandleRestartClicked() => RestartClicked?.Invoke();
    private void HandleResumeClicked() => ResumeClicked?.Invoke();
    private void HandleBgmVolumeChanged(float value) => BgmVolumeChanged?.Invoke(Mathf.Clamp01(value));
    private void HandleSfxVolumeChanged(float value) => SfxVolumeChanged?.Invoke(Mathf.Clamp01(value));

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

        if (_bgmSlider != null)
            _bgmSlider.onValueChanged.RemoveListener(HandleBgmVolumeChanged);

        if (_sfxSlider != null)
            _sfxSlider.onValueChanged.RemoveListener(HandleSfxVolumeChanged);
    }
}
