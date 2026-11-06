using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

public static class ContentCatalogEditor
{
    private const string EnemyCatalogPath = "Assets/Data/Catalogs/EnemyCatalog.asset";
    private const string TowerCatalogPath = "Assets/Data/Catalogs/TowerCatalog.asset";
    private const string RewardCatalogPath = "Assets/Data/Catalogs/RewardCatalog.asset";

    private static readonly string[] EnemyFolders = { "Assets/Script/Enemy/SO" };
    private static readonly string[] TowerFolders = { "Assets/Script/Tower/SO" };
    private static readonly string[] RewardFolders = { "Assets/Script/Reward/SO" };

    [MenuItem("Core9/Catalogs/Refresh Production Content")]
    public static void RefreshProductionContent()
    {
        EnemyCatalog enemies = LoadRequired<EnemyCatalog>(EnemyCatalogPath);
        TowerCatalog towers = LoadRequired<TowerCatalog>(TowerCatalogPath);
        RewardCatalog rewards = LoadRequired<RewardCatalog>(RewardCatalogPath);

        RefreshEnemies(enemies);
        RefreshTowers(towers);
        RefreshRewards(rewards);
        AssetDatabase.SaveAssets();
        Debug.Log("Core9 production catalogs refreshed and saved.");
    }

    [MenuItem("Core9/Catalogs/Validate Production Content")]
    public static void ValidateProductionContent()
    {
        var errors = new List<string>();
        EnemyCatalog enemies = LoadRequired<EnemyCatalog>(EnemyCatalogPath);
        TowerCatalog towers = LoadRequired<TowerCatalog>(TowerCatalogPath);
        RewardCatalog rewards = LoadRequired<RewardCatalog>(RewardCatalogPath);

        if (!enemies.TryValidate(out string enemyError))
            errors.Add(enemyError);

        if (!towers.TryValidate(out string towerError))
            errors.Add(towerError);

        if (!rewards.TryValidate(out string rewardError))
            errors.Add(rewardError);

        if (errors.Count > 0)
            throw new InvalidOperationException(string.Join(Environment.NewLine, errors));

        Debug.Log("Core9 production catalogs are valid.");
    }

    private static void RefreshEnemies(EnemyCatalog catalog)
    {
        List<EnemyCatalogEntry> entries = catalog.Entries
            .Where(entry => entry != null && entry.Config != null)
            .ToList();
        var registered = new HashSet<EnemyConfig>(entries.Select(entry => entry.Config));

        foreach (EnemyConfig config in FindAssets<EnemyConfig>(EnemyFolders))
        {
            if (registered.Contains(config) || !IsComplete(config))
                continue;

            entries.Add(new EnemyCatalogEntry(CreateId(config.name), config));
            registered.Add(config);
        }

        catalog.SetEntries(entries);
        EditorUtility.SetDirty(catalog);
    }

    private static void RefreshTowers(TowerCatalog catalog)
    {
        List<TowerPartData> towers = catalog.TowerParts
            .Where(tower => tower != null)
            .ToList();
        var registered = new HashSet<TowerPartData>(towers);

        foreach (TowerPartData tower in FindAssets<TowerPartData>(TowerFolders))
        {
            if (registered.Contains(tower) || !IsComplete(tower))
                continue;

            towers.Add(tower);
            registered.Add(tower);
        }

        catalog.SetTowerParts(towers);
        EditorUtility.SetDirty(catalog);
    }

    private static void RefreshRewards(RewardCatalog catalog)
    {
        List<RewardData> rewards = catalog.Rewards
            .Where(reward => reward != null)
            .ToList();
        var registered = new HashSet<RewardData>(rewards);

        foreach (RewardData reward in FindAssets<RewardData>(RewardFolders))
        {
            if (registered.Contains(reward) || !IsComplete(reward))
                continue;

            rewards.Add(reward);
            registered.Add(reward);
        }

        catalog.SetRewards(rewards);
        EditorUtility.SetDirty(catalog);
    }

    private static bool IsComplete(EnemyConfig config) =>
        config != null && config.MovementDefinition != null &&
        config.MaxHp > 0f && config.MoveSpeed >= 0f;

    private static bool IsComplete(TowerPartData tower) =>
        tower != null && !string.IsNullOrWhiteSpace(tower.TowerName) &&
        tower.AttackDefinition != null && tower.FireModeDefinition != null;

    private static bool IsComplete(RewardData reward) =>
        reward != null && !string.IsNullOrWhiteSpace(reward.RewardName) &&
        reward.RewardDefinition != null;

    private static List<T> FindAssets<T>(string[] folders) where T : UnityEngine.Object
    {
        return AssetDatabase.FindAssets($"t:{typeof(T).Name}", folders)
            .Select(AssetDatabase.GUIDToAssetPath)
            .OrderBy(path => path, StringComparer.Ordinal)
            .Select(AssetDatabase.LoadAssetAtPath<T>)
            .Where(asset => asset != null)
            .ToList();
    }

    private static T LoadRequired<T>(string path) where T : UnityEngine.Object
    {
        T asset = AssetDatabase.LoadAssetAtPath<T>(path);

        if (asset == null)
            throw new InvalidOperationException($"Required catalog is missing: {path}");

        return asset;
    }

    private static string CreateId(string name)
    {
        var result = new StringBuilder(name.Length);

        for (int i = 0; i < name.Length; i++)
        {
            char character = name[i];

            if (char.IsLetterOrDigit(character))
                result.Append(char.ToLowerInvariant(character));
            else if (result.Length > 0 && result[result.Length - 1] != '-')
                result.Append('-');
        }

        return result.ToString().Trim('-');
    }
}
