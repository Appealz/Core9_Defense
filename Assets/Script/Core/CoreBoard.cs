using UnityEngine;

public class CoreBoard
{
    private const int SlotCount = 9;

    private readonly int[] _hitTargetPriority =
    {
        9, 7, 3, 1, 8, 6, 4, 2, 5
    };

    private readonly CoreSlot[] _slots;
   
    
    public CoreBoard()
    {
        _slots = new CoreSlot[SlotCount];

        for (int i = 0; i < _slots.Length; i++)
        {
            _slots[i] = new CoreSlot(i + 1);
        }

        
    }

    public bool TryGetSlot(int slotNumber, out CoreSlot slot)
    {
        if (slotNumber < 1 || slotNumber > SlotCount)
        {
            slot = null;
            return false;
        }

        slot = _slots[slotNumber - 1];
        return true;
    }

    public bool TryFindHitTargetSlot(out CoreSlot targetSlot)
    {
        for (int i = 0; i < _hitTargetPriority.Length; i++)
        {
            int slotNumber = _hitTargetPriority[i];
            CoreSlot slot = _slots[slotNumber - 1];

            if (!slot.IsOccupied)
                continue;

            targetSlot = slot;
            return true;
        }

        targetSlot = null;
        return false;
    }
}
