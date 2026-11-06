using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TowerCatalog", menuName = "Core9/Catalogs/Tower Catalog")]
public sealed class TowerCatalog : ScriptableObject
{
    [Header("Starting Tower")]
    [SerializeField, Min(0)] private int _initialTowerSlotNumber = 5;
    [SerializeField] private TowerPartData _startingTower;

    [Header("Registered Towers")]
    [SerializeField] private List<TowerPartData> _towerParts = new();

    [Header("Battle Upgrades")]
    [SerializeField] private List<TowerTypeUpgradeData> _towerTypeUpgrades = new();
    [SerializeField] private List<CoreGoldUpgradeData> _coreGoldUpgrades = new();
    [SerializeField] private List<TowerGoldUpgradeData> _towerGoldUpgrades = new();

    [Header("Persistent Upgrades")]
    [SerializeField] private PersistentUpgradeCatalog _persistentUpgrades;

    public int InitialTowerSlotNumber => Math.Max(0, _initialTowerSlotNumber);
    public TowerPartData StartingTower => _startingTower;
    public IReadOnlyList<TowerPartData> TowerParts => _towerParts;
    public IReadOnlyList<TowerTypeUpgradeData> TowerTypeUpgrades => _towerTypeUpgrades;
    public IReadOnlyList<CoreGoldUpgradeData> CoreGoldUpgrades => _coreGoldUpgrades;
    public IReadOnlyList<TowerGoldUpgradeData> TowerGoldUpgrades => _towerGoldUpgrades;
    public PersistentUpgradeCatalog PersistentUpgrades => _persistentUpgrades;

    public bool TryGetTowerPart(TowerType towerType, out TowerPartData towerPart)
    {
        for (int i = 0; i < _towerParts.Count; i++)
        {
            TowerPartData candidate = _towerParts[i];

            if (candidate != null && candidate.TowerType == towerType)
            {
                towerPart = candidate;
                return true;
            }
        }

        towerPart = null;
        return false;
    }

    public bool TryValidate(out string error)
    {
        if (_startingTower == null || _persistentUpgrades == null)
        {
            error = "Tower catalog starting tower or persistent upgrade catalog is missing.";
            return false;
        }

        var types = new HashSet<TowerType>();
        var parts = new HashSet<TowerPartData>();

        for (int i = 0; i < _towerParts.Count; i++)
        {
            TowerPartData part = _towerParts[i];

            if (part == null || !parts.Add(part) || !types.Add(part.TowerType))
            {
                error = $"Tower catalog entry {i} is null or duplicated.";
                return false;
            }
        }

        if (!parts.Contains(_startingTower))
        {
            error = "The starting tower must be registered in the tower catalog.";
            return false;
        }

        error = null;
        return true;
    }

#if UNITY_EDITOR
    public void SetTowerParts(List<TowerPartData> towerParts) =>
        _towerParts = towerParts ?? new List<TowerPartData>();

    private void OnValidate()
    {
        if (!TryValidate(out string error))
            Debug.LogError(error, this);
    }
#endif
}
