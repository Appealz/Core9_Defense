using UnityEngine;
using System;

public class CoreManager
{
    private readonly CoreBoard _coreBoard;

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

        return slot.TryPlaceTower(tower);
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
        if (!_coreBoard.TryGetSlot(slotNumber, out CoreSlot slot))
        {
            removedTower = null;
            return false;
        }

        if (!slot.IsOccupied)
        {
            removedTower = null;
            return false;
        }

        removedTower = slot.RemoveTower();
        return true;
    }

    public bool HasAnyTower()
    {
        return _coreBoard.HasAnyTower();
    }
}