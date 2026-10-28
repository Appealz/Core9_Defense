
using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

public class TowerManager : ICoreGoldUpgradeApplier, ITowerGoldUpgradeApplier, IDisposable
{
    public event Action AllTowersDestroyed;
    public event Action<Tower> TowerRemoved;

    private readonly TowerFactory _towerFactory;
    private readonly CoreManager _coreManager;
    private readonly CoreLayout _coreLayout;
    private readonly TowerStatModifiers _globalStatModifiers;
    private readonly TowerStatModifiers _coreGoldStatModifiers;
    private readonly TowerTypeUpgradeState _typeUpgradeState;
    private readonly List<Tower> _activeTowers = new();

    public TowerManager(
        TowerFactory towerFactory,
        CoreManager coreManager,
        CoreLayout coreLayout,
        TowerStatModifiers globalStatModifiers,
        TowerStatModifiers coreGoldStatModifiers,
        TowerTypeUpgradeState typeUpgradeState)
    {
        _towerFactory = towerFactory;
        _coreManager = coreManager;
        _coreLayout = coreLayout;
        _globalStatModifiers = globalStatModifiers;
        _coreGoldStatModifiers = coreGoldStatModifiers;
        _typeUpgradeState = typeUpgradeState;
        _coreManager.SlotEffectsChanged += HandleSlotEffectsChanged;
        _coreLayout.LayoutChanged += HandleCoreLayoutChanged;
    }

    public async UniTask<bool> TryPlaceTowerAsync(int slotNumber, TowerPartData partData)
    {
        Tower tower = await _towerFactory.CreateAsync(partData);

        if (!_coreManager.TryPlaceTower(slotNumber, tower))
        {
            _towerFactory.Release(tower);
            return false;
        }

        tower.transform.position = _coreLayout.GetSlotPosition(slotNumber);
        tower.Health.Died += () => HandleTowerDied(tower);
        _activeTowers.Add(tower);
        RefreshTowerForSlot(slotNumber);

        return true;
    }

    public bool CanPlaceNextTower()
    {
        return _coreManager.TryFindPlacementSlotNumber(out _);
    }

    public async UniTask<bool> TryPlaceNextTowerAsync(TowerPartData partData)
    {
        if (!_coreManager.TryFindPlacementSlotNumber(out int slotNumber))
            return false;

        return await TryPlaceTowerAsync(slotNumber, partData);
    }

    public bool TryMoveOrSwapTower(int fromSlotNumber, int toSlotNumber)
    {
        if (!_coreManager.TryGetTower(fromSlotNumber, out Tower movingTower))
            return false;

        _coreManager.TryGetTower(toSlotNumber, out Tower targetTower);

        if (!_coreManager.TryMoveOrSwapTower(fromSlotNumber, toSlotNumber))
            return false;

        movingTower.transform.position = _coreLayout.GetSlotPosition(toSlotNumber);

        if (targetTower != null)
            targetTower.transform.position = _coreLayout.GetSlotPosition(fromSlotNumber);

        RefreshTowerForSlot(fromSlotNumber);
        RefreshTowerForSlot(toSlotNumber);

        return true;
    }

    public bool TryApplyGlobalStatUpgrade(TowerStatType statType, StatModifierType modifierType, float value)
    {
        if (!_globalStatModifiers.TryAdd(statType, modifierType, value))
            return false;

        RefreshAllTowers();

        return true;
    }

    public bool CanApplyCoreGoldUpgrade(TowerStatType statType, float flatValue)
    {
        return TowerStatModifiers.CanModify(statType, StatModifierType.Flat, flatValue);
    }

    public bool TryApplyCoreGoldUpgrade(TowerStatType statType, float flatValue)
    {
        if (!CanApplyCoreGoldUpgrade(statType, flatValue) ||
            !_coreGoldStatModifiers.TryAdd(statType, StatModifierType.Flat, flatValue))
        {
            return false;
        }

        try
        {
            RefreshAllTowers();
            return true;
        }
        catch
        {
            _coreGoldStatModifiers.TryRemove(statType, StatModifierType.Flat, flatValue);

            try
            {
                RefreshAllTowers();
            }
            catch
            {
                // Preserve the purchase failure after restoring the modifier state.
            }

            return false;
        }
    }

    public bool IsTowerActive(Tower tower)
    {
        return tower != null && tower.IsAlive && _activeTowers.Contains(tower);
    }

    public bool CanApplyTowerGoldUpgrade(Tower tower, TowerStatType statType, float flatValue)
    {
        return IsTowerActive(tower) &&
            TowerStatModifiers.CanModify(statType, StatModifierType.Flat, flatValue) &&
            (statType == TowerStatType.AttackDamage ||
             statType == TowerStatType.AttackSpeed ||
             statType == TowerStatType.MaxHp);
    }

    public bool TryApplyTowerGoldUpgrade(Tower tower, TowerStatType statType, float flatValue)
    {
        if (!CanApplyTowerGoldUpgrade(tower, statType, flatValue) ||
            !tower.IndividualGoldModifiers.TryAdd(statType, StatModifierType.Flat, flatValue))
        {
            return false;
        }

        try
        {
            if (!_coreManager.TryFindTowerSlotNumber(tower, out int slotNumber))
                throw new InvalidOperationException("The tower is not placed in a core slot.");

            RefreshTowerForSlot(slotNumber, false);
            return true;
        }
        catch
        {
            tower.IndividualGoldModifiers.TryRemove(statType, StatModifierType.Flat, flatValue);

            if (_coreManager.TryFindTowerSlotNumber(tower, out int slotNumber))
            {
                try
                {
                    RefreshTowerForSlot(slotNumber, false);
                }
                catch
                {
                    // Keep the original transaction failure after restoring modifier state.
                }
            }

            return false;
        }
    }

    public bool HasTowerType(TowerType towerType)
    {
        for (int i = 0; i < _activeTowers.Count; i++)
        {
            if (_activeTowers[i] != null && _activeTowers[i].TowerType == towerType)
                return true;
        }

        return false;
    }

    public int GetTowerTypeUpgradeLevel(TowerType towerType) => _typeUpgradeState.GetLevel(towerType);
    public int GetTowerTypeUpgradeMaxLevel(TowerType towerType) => _typeUpgradeState.GetMaxLevel(towerType);

    public bool CanApplyTowerTypeUpgrade(TowerTypeUpgradeData data, int targetLevel)
    {
        return data != null && HasTowerType(data.TowerType) && _typeUpgradeState.CanApply(data, targetLevel);
    }

    public bool TryApplyTowerTypeUpgrade(TowerTypeUpgradeData data, int targetLevel)
    {
        if (!CanApplyTowerTypeUpgrade(data, targetLevel) || !_typeUpgradeState.TryApply(data, targetLevel))
            return false;

        for (int slotNumber = 1; slotNumber <= CoreBoard.SlotCount; slotNumber++)
        {
            if (_coreManager.TryGetTower(slotNumber, out Tower tower) && tower.TowerType == data.TowerType)
                RefreshTowerForSlot(slotNumber, false);
        }

        return true;
    }

    private void HandleSlotEffectsChanged(int slotNumber)
    {
        RefreshTowerForSlot(slotNumber);
    }

    private void HandleCoreLayoutChanged()
    {
        for (int slotNumber = 1; slotNumber <= CoreBoard.SlotCount; slotNumber++)
        {
            if (_coreManager.TryGetTower(slotNumber, out Tower tower))
                tower.transform.position = _coreLayout.GetSlotPosition(slotNumber);
        }
    }

    private void RefreshTowerForSlot(int slotNumber, bool rebindRuntimeEffects = true)
    {
        if (!_coreManager.TryGetTower(slotNumber, out Tower tower) ||
            !_coreManager.TryGetSlotEffects(slotNumber, out IReadOnlyList<CoreSlotEffect> effects))
        {
            return;
        }

        TowerStatModifiers slotModifiers = new TowerStatModifiers();

        for (int i = 0; i < effects.Count; i++)
            effects[i].AddStatModifiers(slotModifiers);

        TowerStatModifiers typeModifiers = _typeUpgradeState.GetStatModifiers(tower.TowerType);
        tower.Stats.ApplyModifiers(
            _globalStatModifiers,
            _coreGoldStatModifiers,
            slotModifiers,
            typeModifiers,
            tower.IndividualGoldModifiers);
        tower.Health.UpdateMaxHp(tower.Stats.MaxHp);
        tower.RefreshAttackEffect();

        if (rebindRuntimeEffects)
            tower.BindSlotEffects(effects);
    }

    private void RefreshAllTowers()
    {
        for (int slotNumber = 1; slotNumber <= CoreBoard.SlotCount; slotNumber++)
            RefreshTowerForSlot(slotNumber, false);
    }

    private void HandleTowerDied(Tower tower)
    {
        if (!_activeTowers.Remove(tower))
            return;

        bool removedFromBoard = _coreManager.TryRemoveTower(tower);

        TowerRemoved?.Invoke(tower);

        if (!removedFromBoard)
            return;

        _towerFactory.Release(tower);

        if (!_coreManager.HasAnyTower())
            AllTowersDestroyed?.Invoke();
    }

    public void Dispose()
    {
        _coreManager.SlotEffectsChanged -= HandleSlotEffectsChanged;
        _coreLayout.LayoutChanged -= HandleCoreLayoutChanged;
    }
}
