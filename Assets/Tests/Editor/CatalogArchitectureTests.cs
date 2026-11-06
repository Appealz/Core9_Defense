using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class CatalogArchitectureTests
{
    private const string GameStartDataPath = "Assets/Script/Game/GameStartData.asset";

    [Test]
    public void GameStartData_SerializesOnlyTheFourDomainCatalogs()
    {
        FieldInfo[] serializedFields = typeof(GameStartData)
            .GetFields(BindingFlags.Instance | BindingFlags.NonPublic)
            .Where(field => field.GetCustomAttribute<SerializeField>() != null)
            .ToArray();

        CollectionAssert.AreEquivalent(
            new[] { "_enemies", "_towers", "_rewards", "_balance" },
            serializedFields.Select(field => field.Name));
    }

    [Test]
    public void ProductionCatalogs_AreCompleteUniqueAndPreserveCounts()
    {
        GameStartData data = LoadStartData();

        Assert.That(data.Enemies.TryValidate(out string enemyError), Is.True, enemyError);
        Assert.That(data.Towers.TryValidate(out string towerError), Is.True, towerError);
        Assert.That(data.Rewards.TryValidate(out string rewardError), Is.True, rewardError);
        Assert.That(data.Balance.TryValidate(out string balanceError), Is.True, balanceError);
        Assert.That(data.Enemies.Entries, Has.Count.EqualTo(6));
        Assert.That(data.Towers.TowerParts, Has.Count.EqualTo(6));
        Assert.That(data.Towers.TowerTypeUpgrades, Has.Count.EqualTo(6));
        Assert.That(data.Rewards.Rewards, Has.Count.EqualTo(41));
    }

    [Test]
    public void WaveConfigs_AreRegisteredAndTestEnemyIsExcluded()
    {
        GameStartData data = LoadStartData();

        foreach (WaveEnemyPhaseDefinition phase in data.Balance.WaveGeneration.EnemyPhases)
            Assert.That(data.Enemies.Contains(phase.EnemyConfig), Is.True, phase.EnemyType.ToString());

        EnemyConfig testEnemy = AssetDatabase.LoadAssetAtPath<EnemyConfig>(
            "Assets/Script/Enemy/TestEnemy.asset");
        Assert.That(testEnemy, Is.Not.Null);
        Assert.That(data.Enemies.Contains(testEnemy), Is.False);
    }

    [Test]
    public void BalanceMigration_PreservesEconomyAndGemValues()
    {
        BalanceCatalog balance = LoadStartData().Balance;
        Assert.That(balance.Economy.ScorePerEnemy, Is.EqualTo(100));
        Assert.That(balance.Economy.BaseCoins, Is.EqualTo(30));
        Assert.That(balance.Economy.CoinsPerWave, Is.EqualTo(10));
        Assert.That(balance.Economy.EarlyExitCoinsPenaltyRate, Is.EqualTo(0.2f));
        Assert.That(balance.FlyingGems.SpawnIntervalMin, Is.EqualTo(5f));
        Assert.That(balance.FlyingGems.SpawnIntervalMax, Is.EqualTo(10f));
        Assert.That(balance.FlyingGems.RewardMin, Is.EqualTo(1));
        Assert.That(balance.FlyingGems.RewardMax, Is.EqualTo(3));
    }

    [Test]
    public void PersistentUpgradeCatalog_NoLongerDuplicatesTowerRegistration()
    {
        PersistentUpgradeCatalog catalog = LoadStartData().Towers.PersistentUpgrades;
        var serializedCatalog = new SerializedObject(catalog);
        Assert.That(serializedCatalog.FindProperty("_towerParts"), Is.Null);
    }

    [Test]
    public void BattleScene_DoesNotContainTowerSpawnTester()
    {
        Scene scene = EditorSceneManager.OpenScene("Assets/Scenes/Battle.unity", OpenSceneMode.Single);
        TowerSpawnTester[] testers = Resources.FindObjectsOfTypeAll<TowerSpawnTester>()
            .Where(tester => tester.gameObject.scene == scene)
            .ToArray();
        Assert.That(testers, Is.Empty);
    }

    private static GameStartData LoadStartData()
    {
        GameStartData data = AssetDatabase.LoadAssetAtPath<GameStartData>(GameStartDataPath);
        Assert.That(data, Is.Not.Null);
        return data;
    }
}
