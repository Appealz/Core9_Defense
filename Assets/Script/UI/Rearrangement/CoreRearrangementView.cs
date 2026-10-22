
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CoreRearrangementView : MonoBehaviour
{
    [SerializeField] private TMP_Text _remainingTimeText;
    [SerializeField] private Button _readyButton;

    public event Action ReadyClicked;

    private void Awake()
    {
        if (_remainingTimeText == null || _readyButton == null)
        {
            Debug.LogError("재배치 UI 참조가 설정되지 않았습니다.", this);
            return;
        }

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

        _remainingTimeText.text = $"Timer : {seconds:00}sec";
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
