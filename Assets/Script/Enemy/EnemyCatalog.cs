using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public sealed class EnemyCatalogEntry
{
    [SerializeField] private string _id;
    [SerializeField] private EnemyConfig _config;

    public string Id => _id;
    public EnemyConfig Config => _config;

#if UNITY_EDITOR
    public EnemyCatalogEntry(string id, EnemyConfig config)
    {
        _id = id;
        _config = config;
    }
#endif
}

[CreateAssetMenu(fileName = "EnemyCatalog", menuName = "Core9/Catalogs/Enemy Catalog")]
public sealed class EnemyCatalog : ScriptableObject
{
    [SerializeField] private List<EnemyCatalogEntry> _entries = new();

    public IReadOnlyList<EnemyCatalogEntry> Entries => _entries;

    public bool Contains(EnemyConfig config)
    {
        if (config == null)
            return false;

        for (int i = 0; i < _entries.Count; i++)
        {
            if (_entries[i] != null && _entries[i].Config == config)
                return true;
        }

        return false;
    }

    public bool TryGet(string id, out EnemyConfig config)
    {
        for (int i = 0; i < _entries.Count; i++)
        {
            EnemyCatalogEntry entry = _entries[i];

            if (entry != null && string.Equals(entry.Id, id, StringComparison.Ordinal))
            {
                config = entry.Config;
                return config != null;
            }
        }

        config = null;
        return false;
    }

    public bool TryValidate(out string error)
    {
        var ids = new HashSet<string>(StringComparer.Ordinal);
        var configs = new HashSet<EnemyConfig>();

        for (int i = 0; i < _entries.Count; i++)
        {
            EnemyCatalogEntry entry = _entries[i];

            if (entry == null || entry.Config == null || string.IsNullOrWhiteSpace(entry.Id))
            {
                error = $"Enemy catalog entry {i} is incomplete.";
                return false;
            }

            if (!ids.Add(entry.Id) || !configs.Add(entry.Config))
            {
                error = $"Enemy catalog entry {i} duplicates an id or config.";
                return false;
            }
        }

        error = null;
        return true;
    }

#if UNITY_EDITOR
    public void SetEntries(List<EnemyCatalogEntry> entries) =>
        _entries = entries ?? new List<EnemyCatalogEntry>();

    private void OnValidate()
    {
        if (!TryValidate(out string error))
            Debug.LogError(error, this);
    }
#endif
}
