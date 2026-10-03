using System;
using UnityEngine;

public class CoreLayout : MonoBehaviour
{
    [SerializeField] private int _gridSize = 3;
    [SerializeField] private Vector2 _towerSize = Vector2.one;
    [SerializeField] private float _spacing = 1f;

    public int GridSize => _gridSize;
    public int SlotCount => _gridSize * _gridSize;

    public Vector3 GetSlotPosition(int slotNumber)
    {
        if (slotNumber < 1 || slotNumber > SlotCount)
            throw new ArgumentOutOfRangeException(nameof(slotNumber));

        int index = slotNumber - 1;

        int column = index % _gridSize;
        int row = index / _gridSize;

        float centerOffset = (_gridSize - 1) * 0.5f;

        float horizontalPitch = _towerSize.x + _spacing;
        float verticalPitch = _towerSize.y + _spacing;

        float x = (column - centerOffset) * horizontalPitch;
        float y = (row - centerOffset) * verticalPitch;

        return transform.position + new Vector3(x, y, 0f);
    }

    public Vector2 GetBoundarySize()
    {
        float width = (_towerSize.x * _gridSize) + (_spacing * (_gridSize - 1));
        float height = (_towerSize.y * _gridSize) + (_spacing * (_gridSize - 1));

        return new Vector2(width, height);
    }
}