using System;
using UnityEngine;

public sealed class UserAudioSettings
{
    public const string DefaultBgmPlayerPrefsKey = "Core9.Settings.BgmVolume";
    public const string DefaultSfxPlayerPrefsKey = "Core9.Settings.SfxVolume";
    public const float DefaultVolume = 1f;

    private readonly string _bgmPlayerPrefsKey;
    private readonly string _sfxPlayerPrefsKey;

    public float BgmVolume { get; private set; }
    public float SfxVolume { get; private set; }

    public event Action<float> BgmVolumeChanged;
    public event Action<float> SfxVolumeChanged;

    public UserAudioSettings(
        string bgmPlayerPrefsKey = DefaultBgmPlayerPrefsKey,
        string sfxPlayerPrefsKey = DefaultSfxPlayerPrefsKey)
    {
        _bgmPlayerPrefsKey = string.IsNullOrWhiteSpace(bgmPlayerPrefsKey)
            ? throw new ArgumentException("A BGM PlayerPrefs key is required.", nameof(bgmPlayerPrefsKey))
            : bgmPlayerPrefsKey;
        _sfxPlayerPrefsKey = string.IsNullOrWhiteSpace(sfxPlayerPrefsKey)
            ? throw new ArgumentException("An SFX PlayerPrefs key is required.", nameof(sfxPlayerPrefsKey))
            : sfxPlayerPrefsKey;

        BgmVolume = ReadVolume(_bgmPlayerPrefsKey);
        SfxVolume = ReadVolume(_sfxPlayerPrefsKey);
    }

    public void SetBgmVolume(float volume)
    {
        float normalizedVolume = NormalizeVolume(volume);

        if (Mathf.Approximately(BgmVolume, normalizedVolume))
            return;

        BgmVolume = normalizedVolume;
        SaveVolume(_bgmPlayerPrefsKey, normalizedVolume);
        BgmVolumeChanged?.Invoke(normalizedVolume);
    }

    public void SetSfxVolume(float volume)
    {
        float normalizedVolume = NormalizeVolume(volume);

        if (Mathf.Approximately(SfxVolume, normalizedVolume))
            return;

        SfxVolume = normalizedVolume;
        SaveVolume(_sfxPlayerPrefsKey, normalizedVolume);
        SfxVolumeChanged?.Invoke(normalizedVolume);
    }

    private static float ReadVolume(string key)
    {
        return NormalizeVolume(PlayerPrefs.GetFloat(key, DefaultVolume));
    }

    private static void SaveVolume(string key, float volume)
    {
        PlayerPrefs.SetFloat(key, volume);
        PlayerPrefs.Save();
    }

    private static float NormalizeVolume(float volume)
    {
        return float.IsNaN(volume) || float.IsInfinity(volume)
            ? DefaultVolume
            : Mathf.Clamp01(volume);
    }
}
