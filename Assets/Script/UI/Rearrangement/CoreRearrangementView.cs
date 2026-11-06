
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CoreRearrangementView : MonoBehaviour
{
    [SerializeField] private TMP_Text _titleText;
    [SerializeField] private TMP_Text _descriptionText;
    [SerializeField] private TMP_Text _remainingTimeText;
    [SerializeField] private TMP_Text _autoAdvanceHintText;
    [SerializeField] private TMP_Text _readyHintText;
    [SerializeField] private Button _readyButton;

    public event Action ReadyClicked;

    public bool IsConfigured =>
        _titleText != null && _descriptionText != null && _remainingTimeText != null &&
        _autoAdvanceHintText != null && _readyHintText != null && _readyButton != null;

    private void Awake()
    {
        if (!IsConfigured)
        {
            Debug.LogError("재배치 UI 참조가 설정되지 않았습니다.", this);
            return;
        }

        _titleText.text = "PREPARATION PHASE";
        _descriptionText.text = "Prepare for the next wave.";
        _autoAdvanceHintText.text = "Next wave starts when time runs out.";
        _readyHintText.text = "When ready, tap READY to start the next wave.";
        _readyButton.onClick.AddListener(HandleReadyClicked);
    }

    public void Show(int remainingSeconds)
    {
        gameObject.SetActive(true);
        SetRemainingSeconds(remainingSeconds);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    public void SetRemainingSeconds(int seconds)
    {
        if (_remainingTimeText == null)
            return;

        int clampedSeconds = Mathf.Max(0, seconds);
        _remainingTimeText.text = $"{clampedSeconds / 60:00}:{clampedSeconds % 60:00}";
    }

    private void HandleReadyClicked()
    {
        ReadyClicked?.Invoke();
    }

    private void OnDestroy()
    {
        if (_readyButton != null)
            _readyButton.onClick.RemoveListener(HandleReadyClicked);
    }
}
