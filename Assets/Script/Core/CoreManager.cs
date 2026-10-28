
using System;
using System.Collections.Generic;

public class CoreManager
{
    public event Action<int> HitTargetSlotChanged;
    public event Action<int> SlotEffectsChanged;

    private readonly CoreBoard _coreBoard;
    private int _currentHitTargetSlotNumber;

    public CoreManager(CoreBoard coreBoard)
    {
        _coreBoard = coreBoard;
    }

    public bool TryPlaceTower(int slotNumber, Tower tower)
    {
        if (tower == null)
            return false;

        if (!_coreBoard.TryGetSlot(slotNumber, out CoreSlot slot))
            return false;

        if (!slot.TryPlaceTower(tower))
            return false;

        UpdateHitTarget();
        return true;
    }

    public bool TryGetTower(int slotNumber, out Tower tower)
    {
        tower = null;

        if (!_coreBoard.TryGetSlot(slotNumber, out CoreSlot slot))
            return false;

        tower = slot.CurrentTower;
        return tower != null;
    }

    public bool TryFindTowerSlotNumber(Tower tower, out int slotNumber)
    {
        slotNumber = 0;

        if (tower == null)
            return false;

        for (int currentSlot = 1; _coreBoard.TryGetSlot(currentSlot, out CoreSlot slot); currentSlot++)
        {
            if (slot.CurrentTower != tower)
                continue;

            slotNumber = currentSlot;
            return true;
        }

        return false;
    }

    public bool TryGetSlotEffects(int slotNumber, out IReadOnlyList<CoreSlotEffect> effects)
    {
        if (!_coreBoard.TryGetSlot(slotNumber, out CoreSlot slot))
        {
            effects = null;
            return false;
        }

        effects = slot.Effects;
        return true;
    }

    public bool CanAddSlotEffects(
        IReadOnlyList<int> slotNumbers,
        IReadOnlyList<CoreSlotEffect> effects)
    {
        if (slotNumbers == null || slotNumbers.Count == 0 || effects == null || effects.Count == 0)
            return false;

        HashSet<int> uniqueSlots = new HashSet<int>();

        for (int i = 0; i < slotNumbers.Count; i++)
        {
            int slotNumber = slotNumbers[i];

            if (!uniqueSlots.Add(slotNumber) ||
                !_coreBoard.TryGetSlot(slotNumber, out CoreSlot slot) ||
                !slot.CanAddEffects(effects))
            {
                return false;
            }
        }

        return true;
    }

    public bool TryAddSlotEffects(
        IReadOnlyList<int> slotNumbers,
        IReadOnlyList<CoreSlotEffect> effects)
    {
        if (!CanAddSlotEffects(slotNumbers, effects))
            return false;

        for (int i = 0; i < slotNumbers.Count; i++)
        {
            int slotNumber = slotNumbers[i];
            _coreBoard.TryGetSlot(slotNumber, out CoreSlot slot);
            slot.TryAddEffects(effects);
            SlotEffectsChanged?.Invoke(slotNumber);
        }

        return true;
    }

    public bool TryFindPlacementSlotNumber(out int slotNumber)
    {
        if (!_coreBoard.TryFindPlacementSlot(out CoreSlot slot))
        {
            slotNumber = 0;
            return false;
        }

        slotNumber = slot.SlotNumber;
        return true;
    }

    public bool TryFindHitTargetSlotNumber(out int slotNumber)
    {
        if (!_coreBoard.TryFindHitTargetSlot(out CoreSlot slot))
        {
            slotNumber = 0;
            return false;
        }

        slotNumber = slot.SlotNumber;
        return true;
    }

    public bool TryFindHitTargetTower(out Tower targetTower)
    {
        if (!_coreBoard.TryFindHitTargetSlot(out CoreSlot targetSlot))
        {
            targetTower = null;
            return false;
        }

        targetTower = targetSlot.CurrentTower;
        return true;
    }

    public bool TryApplyDamage(float damage)
    {
        if (damage <= 0f)
            return false;

        if (!TryFindHitTargetTower(out Tower targetTower) || !targetTower.IsAlive)
            return false;

        targetTower.TakeDamage(damage);
        return true;
    }

    public bool TryRemoveTower(int slotNumber, out Tower removedTower)
    {
        removedTower = null;

        if (!_coreBoard.TryGetSlot(slotNumber, out CoreSlot slot))
            return false;

        if (!slot.IsOccupied)
            return false;

        removedTower = slot.RemoveTower();
        UpdateHitTarget();
        return true;
    }

    public bool TryRemoveTower(Tower tower)
    {
        if (tower == null)
            return false;

        for (int slotNumber = 1; _coreBoard.TryGetSlot(slotNumber, out CoreSlot slot); slotNumber++)
        {
            if (slot.CurrentTower != tower)
                continue;

            slot.RemoveTower();
            UpdateHitTarget();
            return true;
        }

        return false;
    }

    public bool TryMoveOrSwapTower(int fromSlotNumber, int toSlotNumber)
    {
        if (!_coreBoard.TryMoveOrSwapTower(fromSlotNumber, toSlotNumber))
            return false;

        UpdateHitTarget();
        return true;
    }

    public bool HasAnyTower()
    {
        return _coreBoard.HasAnyTower();
    }

    private void UpdateHitTarget()
    {
        TryFindHitTargetSlotNumber(out int slotNumber);

        if (_currentHitTargetSlotNumber == slotNumber)
            return;

        _currentHitTargetSlotNumber = slotNumber;
        HitTargetSlotChanged?.Invoke(slotNumber);
    }
}
