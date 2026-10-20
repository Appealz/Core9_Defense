
using System;

public class CoreManager
{
    public event Action<int> HitTargetSlotChanged;

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
