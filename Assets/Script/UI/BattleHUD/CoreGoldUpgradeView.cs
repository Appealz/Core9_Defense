using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class CoreGoldUpgradeView : MonoBehaviour
{
    [SerializeField] private GameObject _panelRoot;
    [SerializeField] private RectTransform _itemContainer;
    [SerializeField] private GameObject _itemPrefab;

    private readonly List<CoreGoldUpgradeItemView> _items = new();

    public bool IsConfigured => _panelRoot != null && _itemContainer != null && _itemPrefab != null;
    public RectTransform LayoutRect => _panelRoot != null
        ? _panelRoot.transform as RectTransform
        : null;

    public void Build(
        IReadOnlyList<CoreGoldUpgradeData> upgradeData,
        Action<CoreGoldUpgradeData> purchaseRequested)
    {
        Clear();

        if (upgradeData == null)
            return;

        for (int i = 0; i < upgradeData.Count; i++)
        {
            GameObject itemObject = Instantiate(_itemPrefab, _itemContainer);
            CoreGoldUpgradeItemView item = itemObject.GetComponent<CoreGoldUpgradeItemView>();

            if (item == null)
            {
                Destroy(itemObject);
                Debug.LogError("CoreGoldUpgradeItemView component is missing from the item prefab.", _itemPrefab);
                continue;
            }

            item.Initialize(upgradeData[i], purchaseRequested);
            _items.Add(item);
        }
    }

    public void SetVisible(bool isVisible)
    {
        if (_panelRoot.activeSelf != isVisible)
            _panelRoot.SetActive(isVisible);
    }

    public void SetItemState(
        CoreGoldUpgradeData data,
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

            _items[i].SetState(level, totalBonus, nextCost, isMaxLevel, canPurchase);
            return;
        }
    }

    public void Clear()
    {
        for (int i = 0; i < _items.Count; i++)
        {
            if (_items[i] != null)
                Destroy(_items[i].gameObject);
        }

        _items.Clear();
    }
}
