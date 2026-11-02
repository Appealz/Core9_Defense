using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public readonly struct UpgradeRowDisplayData
{
    public TowerStatType StatType { get; }
    public PermanentUpgradeKind Kind { get; }
    public string DisplayName { get; }
    public int Level { get; }
    public string AccumulatedAmount { get; }
    public int NextCost { get; }
    public bool IsUnlocked { get; }
    public bool CanPurchase { get; }
    public int UnlockFlatLevel { get; }

    public UpgradeRowDisplayData(
        TowerStatType statType,
        PermanentUpgradeKind kind,
        string displayName,
        int level,
        string accumulatedAmount,
        int nextCost,
        bool isUnlocked,
        bool canPurchase,
        int unlockFlatLevel)
    {
        StatType = statType;
        Kind = kind;
        DisplayName = displayName;
        Level = level;
        AccumulatedAmount = accumulatedAmount;
        NextCost = nextCost;
        IsUnlocked = isUnlocked;
        CanPurchase = canPurchase;
        UnlockFlatLevel = unlockFlatLevel;
    }
}

public sealed class UpgradeRowView : MonoBehaviour
{
    [SerializeField] private TowerStatType _statType;
    [SerializeField] private PermanentUpgradeKind _kind;
    [SerializeField] private TMP_Text _statNameText;
    [SerializeField] private TMP_Text _levelText;
    [SerializeField] private TMP_Text _amountText;
    [SerializeField] private Button _purchaseButton;
    [SerializeField] private TMP_Text _purchaseButtonText;
    [SerializeField] private GameObject _lockGroup;
    [SerializeField] private TMP_Text _unlockRequirementText;

    private bool _initialized;

    public TowerStatType StatType => _statType;
    public PermanentUpgradeKind Kind => _kind;
    public event Action<PermanentUpgradeKind, TowerStatType> PurchaseClicked;

    public void Configure(
        TowerStatType statType,
        PermanentUpgradeKind kind,
        TMP_Text statNameText,
        TMP_Text levelText,
        TMP_Text amountText,
        Button purchaseButton,
        TMP_Text purchaseButtonText,
        GameObject lockGroup,
        TMP_Text unlockRequirementText)
    {
        _statType = statType;
        _kind = kind;
        _statNameText = statNameText;
        _levelText = levelText;
        _amountText = amountText;
        _purchaseButton = purchaseButton;
        _purchaseButtonText = purchaseButtonText;
        _lockGroup = lockGroup;
        _unlockRequirementText = unlockRequirementText;
    }

    public void Initialize()
    {
        if (_initialized)
            return;

        _initialized = true;
        _purchaseButton.onClick.AddListener(() => PurchaseClicked?.Invoke(_kind, _statType));
    }

    public void Render(UpgradeRowDisplayData data)
    {
        _statNameText.text = data.DisplayName;
        _levelText.text = $"Lv.{data.Level}";
        _amountText.text = data.AccumulatedAmount;
        _lockGroup.SetActive(!data.IsUnlocked);
        _purchaseButton.interactable = data.CanPurchase;
        _purchaseButtonText.text = data.IsUnlocked ? data.NextCost.ToString("N0") : "LOCKED";
        _unlockRequirementText.text = $"Flat Lv.{data.UnlockFlatLevel}";
    }
}
