using System;
using UnityEngine;

public class CoreLayout : MonoBehaviour
{
    public event Action LayoutChanged;

    [SerializeField] private int _gridSize = 3;
    [SerializeField] private Vector2 _towerSize = Vector2.one;
    [SerializeField] private float _spacing = 1f;

    [Header("Automatic Positioning")]
    [Tooltip("World-space offset applied after centering the core in CombatBounds.")]
    [SerializeField] private Vector2 _corePositionOffset = Vector2.zero;

    public int GridSize => _gridSize;
    public int SlotCount => _gridSize * _gridSize;
    public Vector2 PositionOffset => _corePositionOffset;

    public Vector2 GetPositionForCombatCenter(Vector2 combatCenter)
    {
        return combatCenter + _corePositionOffset;
    }

    public void SetCenter(Vector2 center)
    {
        Vector3 currentPosition = transform.position;
        Vector2 targetPosition = GetPositionForCombatCenter(center);

        if (Mathf.Approximately(currentPosition.x, targetPosition.x) &&
            Mathf.Approximately(currentPosition.y, targetPosition.y))
        {
            return;
        }

        transform.position = new Vector3(targetPosition.x, targetPosition.y, currentPosition.z);
        LayoutChanged?.Invoke();
    }

    public Vector3 GetSlotPosition(int slotNumber)
    {
        if (slotNumber < 1 || slotNumber > SlotCount)
            throw new ArgumentOutOfRangeException(nameof(slotNumber));

        int index = slotNumber - 1;
        return GetGridPosition(new Vector2Int(index % _gridSize, index / _gridSize));
    }

    public Vector3 GetGridPosition(Vector2Int gridPosition)
    {
        int column = gridPosition.x;
        int row = gridPosition.y;

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
