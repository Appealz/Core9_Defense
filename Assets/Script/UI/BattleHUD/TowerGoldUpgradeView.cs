using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class TowerGoldUpgradeView : MonoBehaviour
{
    private sealed class ItemEntry
    {
        public TowerGoldUpgradeData Data;
        public CoreGoldUpgradeItemView View;
    }

    [SerializeField] private GameObject _panelRoot;
    [SerializeField] private TMP_Text _towerNameText;
    [SerializeField] private Button _closeButton;
    [SerializeField] private RectTransform _itemContainer;
    [SerializeField] private GameObject _itemPrefab;

    private readonly List<ItemEntry> _items = new();
    private Action<TowerGoldUpgradeData> _purchaseRequested;
    private Action _closeRequested;

    public bool IsConfigured => _panelRoot != null && _towerNameText != null &&
        _closeButton != null && _itemContainer != null && _itemPrefab != null;

    public void Build(
        IReadOnlyList<TowerGoldUpgradeData> upgradeData,
        Action<TowerGoldUpgradeData> purchaseRequested,
        Action closeRequested)
    {
        Clear();
        _purchaseRequested = purchaseRequested;
        _closeRequested = closeRequested;
        _closeButton.onClick.RemoveListener(HandleCloseClicked);
        _closeButton.onClick.AddListener(HandleCloseClicked);

        if (upgradeData == null)
            return;

        for (int i = 0; i < upgradeData.Count; i++)
        {
            TowerGoldUpgradeData data = upgradeData[i];
            GameObject itemObject = Instantiate(_itemPrefab, _itemContainer);
            CoreGoldUpgradeItemView itemView = itemObject.GetComponent<CoreGoldUpgradeItemView>();

            if (itemView == null)
            {
                Destroy(itemObject);
                Debug.LogError("CoreGoldUpgradeItemView is missing from the shared gold upgrade item prefab.", _itemPrefab);
                continue;
            }

            itemView.Initialize(data.DisplayName, () => _purchaseRequested?.Invoke(data));
            _items.Add(new ItemEntry { Data = data, View = itemView });
        }
    }

    public void ShowTower(string towerName)
    {
        _towerNameText.text = string.IsNullOrWhiteSpace(towerName) ? "Tower" : towerName;
        SetVisible(true);
    }

    public void SetVisible(bool isVisible)
    {
        if (_panelRoot.activeSelf != isVisible)
            _panelRoot.SetActive(isVisible);
    }

    public void SetItemState(
        TowerGoldUpgradeData data,
        int level,
        string totalBonus,
        int nextCost,
        bool isMaxLevel,
        bool canPurchase)
    {
        for (int i = 0; i < _items.Count; i++)
        {
            if (_items[i].Data != data)
                continue;

            _items[i].View.SetState(level, totalBonus, nextCost, isMaxLevel, canPurchase);
            return;
        }
    }

    public void Clear()
    {
        for (int i = 0; i < _items.Count; i++)
        {
            if (_items[i].View != null)
                Destroy(_items[i].View.gameObject);
        }

        _items.Clear();
    }

    private void HandleCloseClicked()
    {
        _closeRequested?.Invoke();
    }

    private void OnDestroy()
    {
        if (_closeButton != null)
            _closeButton.onClick.RemoveListener(HandleCloseClicked);
    }
}
