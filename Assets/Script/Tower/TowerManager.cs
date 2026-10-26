
using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

public class TowerManager
{
    public event Action AllTowersDestroyed;

    private readonly TowerFactory _towerFactory;
    private readonly CoreManager _coreManager;
    private readonly CoreLayout _coreLayout;
    private readonly TowerStatModifiers _globalStatModifiers;
    private readonly List<Tower> _activeTowers = new();

    public TowerManager(TowerFactory towerFactory, CoreManager coreManager, CoreLayout coreLayout, TowerStatModifiers globalStatModifiers)
    {
        _towerFactory = towerFactory;
        _coreManager = coreManager;
        _coreLayout = coreLayout;
        _globalStatModifiers = globalStatModifiers;
        _coreManager.SlotEffectsChanged += HandleSlotEffectsChanged;
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

        for (int slotNumber = 1; slotNumber <= CoreBoard.SlotCount; slotNumber++)
            RefreshTowerForSlot(slotNumber, false);

        return true;
    }

    private void HandleSlotEffectsChanged(int slotNumber)
    {
        RefreshTowerForSlot(slotNumber);
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

        tower.Stats.ApplyModifiers(_globalStatModifiers, slotModifiers);
        tower.Health.UpdateMaxHp(tower.Stats.MaxHp);

        if (rebindRuntimeEffects)
            tower.BindSlotEffects(effects);
    }

    private void HandleTowerDied(Tower tower)
    {
        _activeTowers.Remove(tower);

        if (!_coreManager.TryRemoveTower(tower))
            return;

        _towerFactory.Release(tower);

        if (!_coreManager.HasAnyTower())
            AllTowersDestroyed?.Invoke();
    }
}
