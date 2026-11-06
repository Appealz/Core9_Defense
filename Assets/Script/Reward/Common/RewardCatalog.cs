using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RewardCatalog", menuName = "Core9/Catalogs/Reward Catalog")]
public sealed class RewardCatalog : ScriptableObject
{
    [SerializeField] private List<RewardData> _rewards = new();
    [SerializeField] private RewardSelectionSettings _selectionSettings = new();

    public IReadOnlyList<RewardData> Rewards => _rewards;
    public RewardSelectionSettings SelectionSettings => _selectionSettings;

    public bool TryValidate(out string error)
    {
        var registered = new HashSet<RewardData>();

        for (int i = 0; i < _rewards.Count; i++)
        {
            if (_rewards[i] == null || !registered.Add(_rewards[i]))
            {
                error = $"Reward catalog entry {i} is null or duplicated.";
                return false;
            }
        }

        if (_selectionSettings == null)
        {
            error = "Reward selection settings are missing.";
            return false;
        }

        return _selectionSettings.TryValidate(out error);
    }

#if UNITY_EDITOR
    public void SetRewards(List<RewardData> rewards) =>
        _rewards = rewards ?? new List<RewardData>();

    private void OnValidate()
    {
        if (!TryValidate(out string error))
            Debug.LogError(error, this);
    }
#endif
}
