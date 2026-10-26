using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CoreSlotPattern", menuName = "Core9/Core Slot Pattern")]
public sealed class CoreSlotPatternData : ScriptableObject
{
    [SerializeField] private string _patternName = "Single";
    [Tooltip("Fixed offsets from the selected anchor slot. Rotation and reflection are intentionally unsupported.")]
    [SerializeField] private List<Vector2Int> _cells = new() { Vector2Int.zero };

    public string PatternName => string.IsNullOrWhiteSpace(_patternName) ? name : _patternName;
    public IReadOnlyList<Vector2Int> Cells => _cells;

    public bool TryGetSlotNumbers(int anchorSlotNumber, int gridSize, List<int> results)
    {
        results.Clear();

        List<Vector2Int> gridCells = new List<Vector2Int>(_cells.Count);

        if (!TryGetGridCells(anchorSlotNumber, gridSize, gridCells, out bool allInside) || !allInside)
            return false;

        for (int i = 0; i < gridCells.Count; i++)
        {
            Vector2Int cell = gridCells[i];
            results.Add(cell.y * gridSize + cell.x + 1);
        }

        return true;
    }

    public bool TryGetGridCells(
        int anchorSlotNumber,
        int gridSize,
        List<Vector2Int> results,
        out bool allInside)
    {
        results.Clear();
        allInside = false;

        if (gridSize <= 0 || anchorSlotNumber < 1 || anchorSlotNumber > gridSize * gridSize || _cells.Count == 0)
            return false;

        int anchorIndex = anchorSlotNumber - 1;
        int anchorColumn = anchorIndex % gridSize;
        int anchorRow = anchorIndex / gridSize;
        allInside = true;

        for (int i = 0; i < _cells.Count; i++)
        {
            Vector2Int cell = _cells[i];
            int column = anchorColumn + cell.x;
            int row = anchorRow + cell.y;
            Vector2Int gridCell = new Vector2Int(column, row);

            if (results.Contains(gridCell))
                return false;

            results.Add(gridCell);

            if (column < 0 || column >= gridSize || row < 0 || row >= gridSize)
                allInside = false;
        }

        return true;
    }
}
