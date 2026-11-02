using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class UpgradeView : MonoBehaviour
{
    private static readonly Color ActiveTabColor = new Color(0.96f, 0.97f, 1f, 1f);
    private static readonly Color IdleTabColor = new Color(0.13f, 0.16f, 0.22f, 1f);

    [SerializeField] private GameObject _root;
    [SerializeField] private Button _backButton;
    [SerializeField] private TMP_Text _currencyText;
    [SerializeField] private Button _coreTabButton;
    [SerializeField] private Image _coreTabImage;
    [SerializeField] private TMP_Text _coreTabText;
    [SerializeField] private Button _towerTabButton;
    [SerializeField] private Image _towerTabImage;
    [SerializeField] private TMP_Text _towerTabText;
    [SerializeField] private GameObject _towerSelectorRoot;
    [SerializeField] private TMP_Text _targetNameText;
    [SerializeField] private TowerSelectionItemView[] _towerItems;
    [SerializeField] private UpgradeRowView[] _flatRows;
    [SerializeField] private UpgradeRowView[] _rateRows;

    private bool _initialized;

    public event Action BackClicked;
    public event Action<PermanentUpgradeTarget> TargetClicked;
    public event Action<TowerType> TowerClicked;
    public event Action<PermanentUpgradeKind, TowerStatType> PurchaseClicked;

    public void Configure(
        GameObject root,
        Button backButton,
        TMP_Text currencyText,
        Button coreTabButton,
        Image coreTabImage,
        TMP_Text coreTabText,
        Button towerTabButton,
        Image towerTabImage,
        TMP_Text towerTabText,
        GameObject towerSelectorRoot,
        TMP_Text targetNameText,
        TowerSelectionItemView[] towerItems,
        UpgradeRowView[] flatRows,
        UpgradeRowView[] rateRows)
    {
        _root = root;
        _backButton = backButton;
        _currencyText = currencyText;
        _coreTabButton = coreTabButton;
        _coreTabImage = coreTabImage;
        _coreTabText = coreTabText;
        _towerTabButton = towerTabButton;
        _towerTabImage = towerTabImage;
        _towerTabText = towerTabText;
        _towerSelectorRoot = towerSelectorRoot;
        _targetNameText = targetNameText;
        _towerItems = towerItems;
        _flatRows = flatRows;
        _rateRows = rateRows;
    }

    public void Initialize()
    {
        if (_initialized)
            return;

        _initialized = true;
        _backButton.onClick.AddListener(() => BackClicked?.Invoke());
        _coreTabButton.onClick.AddListener(() => TargetClicked?.Invoke(PermanentUpgradeTarget.Core));
        _towerTabButton.onClick.AddListener(() => TargetClicked?.Invoke(PermanentUpgradeTarget.Tower));

        for (int i = 0; i < _towerItems.Length; i++)
        {
            _towerItems[i].Initialize();
            _towerItems[i].Clicked += HandleTowerClicked;
        }

        InitializeRows(_flatRows);
        InitializeRows(_rateRows);
    }

    public void SetVisible(bool visible)
    {
        if (_root != null)
            _root.SetActive(visible);
    }

    public void Render(
        PermanentUpgradeTarget target,
        TowerType selectedTower,
        int balance,
        UpgradeRowDisplayData[] flatRows,
        UpgradeRowDisplayData[] rateRows)
    {
        bool coreSelected = target == PermanentUpgradeTarget.Core;
        _currencyText.text = balance.ToString("N0");
        _coreTabImage.color = coreSelected ? ActiveTabColor : IdleTabColor;
        _towerTabImage.color = coreSelected ? IdleTabColor : ActiveTabColor;
        _coreTabText.color = coreSelected ? Color.black : Color.white;
        _towerTabText.color = coreSelected ? Color.white : Color.black;
        _towerSelectorRoot.SetActive(!coreSelected);
        _targetNameText.text = coreSelected ? "ALL TOWERS" : selectedTower.ToString().ToUpperInvariant();

        for (int i = 0; i < _towerItems.Length; i++)
            _towerItems[i].SetSelected(_towerItems[i].TowerType == selectedTower);

        RenderRows(_flatRows, flatRows);
        RenderRows(_rateRows, rateRows);
    }

    private void InitializeRows(UpgradeRowView[] rows)
    {
        for (int i = 0; i < rows.Length; i++)
        {
            rows[i].Initialize();
            rows[i].PurchaseClicked += HandlePurchaseClicked;
        }
    }

    private static void RenderRows(UpgradeRowView[] rowViews, UpgradeRowDisplayData[] rows)
    {
        int count = Mathf.Min(rowViews.Length, rows.Length);

        for (int i = 0; i < count; i++)
            rowViews[i].Render(rows[i]);
    }

    private void HandleTowerClicked(TowerType towerType) => TowerClicked?.Invoke(towerType);

    private void HandlePurchaseClicked(PermanentUpgradeKind kind, TowerStatType statType)
    {
        PurchaseClicked?.Invoke(kind, statType);
    }
}
