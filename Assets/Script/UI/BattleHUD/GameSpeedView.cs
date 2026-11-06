using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class GameSpeedView : MonoBehaviour
{
    [SerializeField] private Button _decreaseButton;
    [SerializeField] private TMP_Text _speedText;
    [SerializeField] private Button _increaseButton;
    [SerializeField] private GameObject _tripleSpeedLockIndicator;

    private bool _isInitialized;

    public event Action DecreaseClicked;
    public event Action IncreaseClicked;

    public bool IsConfigured =>
        _decreaseButton != null && _speedText != null &&
        _increaseButton != null && _tripleSpeedLockIndicator != null;

    public void Initialize()
    {
        if (_isInitialized || !IsConfigured)
            return;

        _decreaseButton.onClick.AddListener(HandleDecreaseClicked);
        _increaseButton.onClick.AddListener(HandleIncreaseClicked);
        _isInitialized = true;
    }

    public void SetState(
        int speed,
        bool canDecrease,
        bool canIncrease,
        bool canInteract,
        bool isTripleSpeedLocked)
    {
        _speedText.text = $"{speed}x";
        _decreaseButton.interactable = canInteract && canDecrease;
        _increaseButton.interactable = canInteract && canIncrease;
        _tripleSpeedLockIndicator.SetActive(isTripleSpeedLocked);
    }

    public void SetVisible(bool isVisible)
    {
        if (gameObject.activeSelf != isVisible)
            gameObject.SetActive(isVisible);
    }

    private void HandleDecreaseClicked() => DecreaseClicked?.Invoke();
    private void HandleIncreaseClicked() => IncreaseClicked?.Invoke();

    private void OnDestroy()
    {
        if (!_isInitialized)
            return;

        _decreaseButton.onClick.RemoveListener(HandleDecreaseClicked);
        _increaseButton.onClick.RemoveListener(HandleIncreaseClicked);
    }
}
