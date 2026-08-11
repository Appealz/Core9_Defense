using System;

public sealed class CoreSlot
{
    public int SlotNumber { get; }

    public Tower CurrentTower { get; private set; }

    public bool IsOccupied => CurrentTower != null;

    public CoreSlot(int slotNumber)
    {
        SlotNumber = slotNumber;
    }

    public bool TryPlaceTower(Tower tower)
    {
        if (tower == null)
            return false;

        if (IsOccupied)
            return false;

        CurrentTower = tower;
        return true;
    }

    public Tower RemoveTower()
    {
        Tower removedTower = CurrentTower;
        CurrentTower = null;

        return removedTower;
    }
}