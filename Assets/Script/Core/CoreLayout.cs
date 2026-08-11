using UnityEngine;

public class CoreLayout : MonoBehaviour
{
    [SerializeField]
    private float _slotSpacing = 2f;

    public Vector3 GetSlotPosition(int slotNumber)
    {
        int index = slotNumber - 1;

        int column = index % 3;
        int row = index / 3;

        float x = (column - 1) * _slotSpacing;
        float y = (row - 1) * _slotSpacing;

        return transform.position + new Vector3(x, y, 0f);
    }
}
