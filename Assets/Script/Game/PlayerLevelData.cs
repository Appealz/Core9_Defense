using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerLevelData", menuName = "Scriptable Objects/Player Level Data")]
public sealed class PlayerLevelData : ScriptableObject
{
    [SerializeField] private List<int> _requiredExperienceByLevel = new() { 10, 15, 20, 25, 30 };
    [SerializeField, Min(1)] private int _fallbackInitialRequirement = 10;
    [SerializeField, Min(0)] private int _requirementIncreaseBeyondTable = 5;

    public int GetRequiredExperience(int currentLevel)
    {
        int safeLevel = Mathf.Max(1, currentLevel);
        int tableIndex = safeLevel - 1;

        if (tableIndex < _requiredExperienceByLevel.Count)
            return Mathf.Max(1, _requiredExperienceByLevel[tableIndex]);

        int baseRequirement = _requiredExperienceByLevel.Count > 0
            ? Mathf.Max(1, _requiredExperienceByLevel[_requiredExperienceByLevel.Count - 1])
            : Mathf.Max(1, _fallbackInitialRequirement);
        int levelsBeyondTable = _requiredExperienceByLevel.Count > 0
            ? tableIndex - _requiredExperienceByLevel.Count + 1
            : tableIndex;
        long requirement = baseRequirement + (long)_requirementIncreaseBeyondTable * levelsBeyondTable;
        return (int)Mathf.Clamp(requirement, 1L, int.MaxValue);
    }
}
