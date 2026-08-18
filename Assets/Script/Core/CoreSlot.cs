using System;
using UnityEngine;
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
        Debug.Log($"TryPlaceTower 호출 - 슬롯: {SlotNumber}, Tower null: {tower == null}, 점유 상태: {IsOccupied}");
        if (tower == null)
            return false;

        if (IsOccupied)
            return false;

        CurrentTower = tower;

        Debug.Log($"타워 배치 완료 - 슬롯: {SlotNumber}, CurrentTower null: {CurrentTower == null}, 점유 상태: {IsOccupied}");

        return true;
    }

    public Tower RemoveTower()
    {
        Tower removedTower = CurrentTower;
        CurrentTower = null;

        return removedTower;
    }
}