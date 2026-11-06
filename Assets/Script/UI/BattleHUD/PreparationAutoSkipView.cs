using System;
using UnityEngine;
using UnityEngine.UI;

public sealed class PreparationAutoSkipView : MonoBehaviour
{
    [SerializeField] private GameObject _battleToggleRoot;
    [SerializeField] private Toggle _battleToggle;
    [SerializeField] private Toggle _rewardSelectionToggle;
    [SerializeField] private Toggle _rearrangementToggle;

    private bool _isInitialized;

    public event Action<bool> ValueChanged;

    public bool IsConfigured =>
        _battleToggleRoot != null && _battleToggle != null &&
        _rewardSelectionToggle != null && _rearrangementToggle != null;

    public void Initialize()
    {
        if (_isInitialized || !IsConfigured)
            return;

        _battleToggle.onValueChanged.AddListener(HandleValueChanged);
        _rewardSelectionToggle.onValueChanged.AddListener(HandleValueChanged);
        _rearrangementToggle.onValueChanged.AddListener(HandleValueChanged);
        _isInitialized = true;
    }

    public void SetValue(bool isEnabled)
    {
        _battleToggle.SetIsOnWithoutNotify(isEnabled);
        _rewardSelectionToggle.SetIsOnWithoutNotify(isEnabled);
        _rearrangementToggle.SetIsOnWithoutNotify(isEnabled);
    }

    public void SetBattleToggleState(bool isVisible, bool isInteractable)
    {
        _battleToggle.interactable = isInteractable;

        if (_battleToggleRoot.activeSelf != isVisible)
            _battleToggleRoot.SetActive(isVisible);
    }

    private void HandleValueChanged(bool isEnabled)
    {
        ValueChanged?.Invoke(isEnabled);
    }

    private void OnDestroy()
    {
        if (!_isInitialized)
            return;

        _battleToggle.onValueChanged.RemoveListener(HandleValueChanged);
        _rewardSelectionToggle.onValueChanged.RemoveListener(HandleValueChanged);
        _rearrangementToggle.onValueChanged.RemoveListener(HandleValueChanged);
    }
}
