using System;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class BattleExitConfirmationView : MonoBehaviour
{
    [SerializeField] private GameObject _root;
    [SerializeField] private TMP_Text _titleText;
    [SerializeField] private TMP_Text _descriptionText;
    [SerializeField] private TMP_Text _rewardLabelsText;
    [SerializeField] private TMP_Text _rewardValuesText;
    [SerializeField] private Button _cancelButton;
    [SerializeField] private Button _confirmButton;
    [SerializeField] private TMP_Text _confirmButtonLabel;

    private bool _isInitialized;

    public event Action CancelClicked;
    public event Action ConfirmClicked;

    public bool IsConfigured =>
        _root != null && _titleText != null && _descriptionText != null &&
        _rewardLabelsText != null && _rewardValuesText != null &&
        _cancelButton != null && _confirmButton != null && _confirmButtonLabel != null;

    public bool IsVisible => _root != null && _root.activeSelf;

    public void Initialize()
    {
        if (_isInitialized || !IsConfigured)
            return;

        _cancelButton.onClick.AddListener(HandleCancelClicked);
        _confirmButton.onClick.AddListener(HandleConfirmClicked);
        _root.SetActive(false);
        _isInitialized = true;
    }

    public void Show(
        string title,
        string description,
        string confirmButtonLabel,
        EarlyExitRewardQuote rewardQuote)
    {
        _titleText.text = title;
        _descriptionText.text = description;
        _confirmButtonLabel.text = confirmButtonLabel;
        _rewardLabelsText.text =
            $"Coins Earned\nExit Penalty ({rewardQuote.PenaltyPercent}%)\nCoins Received";
        _rewardValuesText.text = string.Concat(
            FormatNumber(rewardQuote.EarnedCoins),
            "\n-",
            FormatNumber(rewardQuote.PenaltyCoins),
            "\n",
            FormatNumber(rewardQuote.ReceivedCoins));
        SetButtonsInteractable(true);
        _root.transform.SetAsLastSibling();
        _root.SetActive(true);
    }

    public void Hide()
    {
        if (_root != null)
            _root.SetActive(false);
    }

    public void SetButtonsInteractable(bool isInteractable)
    {
        if (_cancelButton != null)
            _cancelButton.interactable = isInteractable;

        if (_confirmButton != null)
            _confirmButton.interactable = isInteractable;
    }

    private static string FormatNumber(int value) =>
        value.ToString("N0", CultureInfo.InvariantCulture);

    private void HandleCancelClicked() => CancelClicked?.Invoke();
    private void HandleConfirmClicked() => ConfirmClicked?.Invoke();

    private void OnDestroy()
    {
        if (!_isInitialized)
            return;

        _cancelButton.onClick.RemoveListener(HandleCancelClicked);
        _confirmButton.onClick.RemoveListener(HandleConfirmClicked);
    }
}
