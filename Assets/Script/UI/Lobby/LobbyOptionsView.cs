using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class LobbyOptionsView : MonoBehaviour
{
    [SerializeField] private GameObject _root;
    [SerializeField] private Slider _bgmSlider;
    [SerializeField] private TMP_Text _bgmValueText;
    [SerializeField] private Slider _sfxSlider;
    [SerializeField] private TMP_Text _sfxValueText;
    [SerializeField] private TMP_Text _languageValueText;
    [SerializeField] private TMP_Text _versionValueText;
    [SerializeField] private Button _closeButton;

    private bool _isInitialized;

    public event Action CloseClicked;
    public event Action<float> BgmVolumeChanged;
    public event Action<float> SfxVolumeChanged;

    public bool IsConfigured =>
        _root != null && _bgmSlider != null && _bgmValueText != null &&
        _sfxSlider != null && _sfxValueText != null &&
        _languageValueText != null && _versionValueText != null &&
        _closeButton != null;

    public bool IsVisible => _root != null && _root.activeSelf;

    public void Initialize()
    {
        if (_isInitialized || !IsConfigured)
            return;

        ConfigureSlider(_bgmSlider);
        ConfigureSlider(_sfxSlider);
        _bgmSlider.onValueChanged.AddListener(HandleBgmVolumeChanged);
        _sfxSlider.onValueChanged.AddListener(HandleSfxVolumeChanged);
        _closeButton.onClick.AddListener(HandleCloseClicked);
        _languageValueText.text = "English";
        _versionValueText.text = FormatVersion(Application.version);
        _isInitialized = true;
    }

    public void SetVisible(bool isVisible)
    {
        if (_root == null)
            return;

        if (isVisible)
            _root.transform.SetAsLastSibling();

        if (_root.activeSelf != isVisible)
            _root.SetActive(isVisible);
    }

    public void Render(float bgmVolume, float sfxVolume)
    {
        float normalizedBgmVolume = Mathf.Clamp01(bgmVolume);
        float normalizedSfxVolume = Mathf.Clamp01(sfxVolume);
        _bgmSlider.SetValueWithoutNotify(normalizedBgmVolume);
        _sfxSlider.SetValueWithoutNotify(normalizedSfxVolume);
        SetPercentage(_bgmValueText, normalizedBgmVolume);
        SetPercentage(_sfxValueText, normalizedSfxVolume);
        _languageValueText.text = "English";
        _versionValueText.text = FormatVersion(Application.version);
    }

    private static void ConfigureSlider(Slider slider)
    {
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.wholeNumbers = false;
        slider.interactable = true;
    }

    private void HandleBgmVolumeChanged(float volume)
    {
        SetPercentage(_bgmValueText, volume);
        BgmVolumeChanged?.Invoke(Mathf.Clamp01(volume));
    }

    private void HandleSfxVolumeChanged(float volume)
    {
        SetPercentage(_sfxValueText, volume);
        SfxVolumeChanged?.Invoke(Mathf.Clamp01(volume));
    }

    private void HandleCloseClicked() => CloseClicked?.Invoke();

    private static void SetPercentage(TMP_Text valueText, float volume)
    {
        valueText.text = $"{Mathf.RoundToInt(Mathf.Clamp01(volume) * 100f)}%";
    }

    private static string FormatVersion(string version)
    {
        string normalizedVersion = string.IsNullOrWhiteSpace(version) ? "0.0.0" : version.Trim();
        return normalizedVersion.StartsWith("v", StringComparison.OrdinalIgnoreCase)
            ? normalizedVersion
            : $"v{normalizedVersion}";
    }

    private void OnDestroy()
    {
        if (!_isInitialized)
            return;

        _bgmSlider.onValueChanged.RemoveListener(HandleBgmVolumeChanged);
        _sfxSlider.onValueChanged.RemoveListener(HandleSfxVolumeChanged);
        _closeButton.onClick.RemoveListener(HandleCloseClicked);
    }
}
