using System;
using System.Collections.Generic;
using UnityEngine;
public sealed class CoreSlot
{
    private readonly List<CoreSlotEffect> _effects = new();

    public int SlotNumber { get; }

    public Tower CurrentTower { get; private set; }

    public bool IsOccupied => CurrentTower != null;
    public IReadOnlyList<CoreSlotEffect> Effects => _effects;

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

    public bool CanAddEffects(IReadOnlyList<CoreSlotEffect> effects)
    {
        if (effects == null || effects.Count == 0)
            return false;

        List<CoreSlotEffect> combinedEffects = new List<CoreSlotEffect>(_effects);

        for (int i = 0; i < effects.Count; i++)
        {
            CoreSlotEffect effect = effects[i];

            if (effect == null || !effect.CanAddTo(combinedEffects))
                return false;

            combinedEffects.Add(effect);
        }

        return true;
    }

    public bool TryAddEffects(IReadOnlyList<CoreSlotEffect> effects)
    {
        if (!CanAddEffects(effects))
            return false;

        for (int i = 0; i < effects.Count; i++)
            _effects.Add(effects[i]);

        return true;
    }
}
