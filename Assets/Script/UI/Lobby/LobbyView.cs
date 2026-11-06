using System;
using UnityEngine;
using UnityEngine.UI;

public sealed class LobbyView : MonoBehaviour
{
    [SerializeField] private GameObject _root;
    [SerializeField] private Button _playButton;
    [SerializeField] private Button _upgradeButton;
    [SerializeField] private Button _rankButton;
    [SerializeField] private Button _settingsButton;

    private bool _initialized;

    public event Action PlayClicked;
    public event Action UpgradeClicked;
    public event Action OptionsClicked;

    public void Configure(
        GameObject root,
        Button playButton,
        Button upgradeButton,
        Button rankButton,
        Button settingsButton)
    {
        _root = root;
        _playButton = playButton;
        _upgradeButton = upgradeButton;
        _rankButton = rankButton;
        _settingsButton = settingsButton;
    }

    public void Initialize()
    {
        if (_initialized)
            return;

        _initialized = true;
        _playButton.onClick.AddListener(() => PlayClicked?.Invoke());
        _upgradeButton.onClick.AddListener(() => UpgradeClicked?.Invoke());
        _settingsButton.onClick.AddListener(() => OptionsClicked?.Invoke());

        // Rank is intentionally presentation-only for this milestone.
        _rankButton.interactable = true;
    }

    public void SetVisible(bool visible)
    {
        if (_root != null)
            _root.SetActive(visible);
    }
}
