using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class CoreGoldUpgradeItemView : MonoBehaviour
{
    [SerializeField] private TMP_Text _nameText;
    [SerializeField] private TMP_Text _levelText;
    [SerializeField] private TMP_Text _totalBonusText;
    [SerializeField] private TMP_Text _costText;
    [SerializeField] private Image _cardBackground;
    [SerializeField] private Button _cardButton;

    private static readonly Color PurchasableColor = new(0.94f, 0.95f, 0.97f, 1f);
    private static readonly Color UnavailableColor = new(0.68f, 0.70f, 0.74f, 1f);
    private static readonly Color MaxLevelColor = new(0.46f, 0.48f, 0.52f, 1f);

    private CoreGoldUpgradeData _data;
    private Action<CoreGoldUpgradeData> _purchaseRequested;
    private Action _genericPurchaseRequested;
    private string _displayName;

    public CoreGoldUpgradeData Data => _data;

    public void Initialize(
        CoreGoldUpgradeData data,
        Action<CoreGoldUpgradeData> purchaseRequested)
    {
        _data = data;
        _displayName = data != null ? data.DisplayName : string.Empty;
        _purchaseRequested = purchaseRequested;
        _genericPurchaseRequested = null;
        _cardButton.onClick.RemoveListener(HandlePurchaseClicked);
        _cardButton.onClick.AddListener(HandlePurchaseClicked);
    }

    public void Initialize(string displayName, Action purchaseRequested)
    {
        _data = null;
        _displayName = displayName ?? string.Empty;
        _purchaseRequested = null;
        _genericPurchaseRequested = purchaseRequested;
        _cardButton.onClick.RemoveListener(HandlePurchaseClicked);
        _cardButton.onClick.AddListener(HandlePurchaseClicked);
    }

    public void SetState(
        int level,
        string totalBonus,
        int nextCost,
        bool isMaxLevel,
        bool canPurchase)
    {
        _nameText.text = _displayName;
        _levelText.text = $"Lv. {level:N0}";
        _totalBonusText.text = $"Total {totalBonus}";
        _costText.text = isMaxLevel ? "MAX LEVEL" : $"Cost {nextCost:N0}";
        _cardBackground.color = isMaxLevel
            ? MaxLevelColor
            : canPurchase ? PurchasableColor : UnavailableColor;
        _cardButton.interactable = canPurchase;
    }

    private void HandlePurchaseClicked()
    {
        if (!_cardButton.interactable)
            return;

        if (_data != null)
            _purchaseRequested?.Invoke(_data);
        else
            _genericPurchaseRequested?.Invoke();
    }

    private void OnDestroy()
    {
        if (_cardButton != null)
            _cardButton.onClick.RemoveListener(HandlePurchaseClicked);
    }
}
