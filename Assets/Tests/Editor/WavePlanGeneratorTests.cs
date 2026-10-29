using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

public sealed class WavePlanGeneratorTests
{
    private const string SettingsPath = "Assets/Script/Wave/WaveGenerationSettings.asset";

    private WaveGenerationSettings _settings;
    private WavePlanGenerator _generator;

    [SetUp]
    public void SetUp()
    {
        _settings = AssetDatabase.LoadAssetAtPath<WaveGenerationSettings>(SettingsPath);
        Assert.That(_settings, Is.Not.Null, $"Missing test settings at {SettingsPath}");
        _generator = new WavePlanGenerator(_settings);
    }

    [TestCase(1, 20)]
    [TestCase(2, 21)]
    [TestCase(3, 22)]
    [TestCase(4, 25)]
    [TestCase(5, 30)]
    [TestCase(6, 31)]
    [TestCase(7, 32)]
    [TestCase(8, 34)]
    [TestCase(9, 37)]
    [TestCase(10, 40)]
    [TestCase(11, 41)]
    [TestCase(12, 42)]
    public void Difficulty_ReturnsExpectedEnemyCount(int waveNumber, int expectedCount)
    {
        Assert.That(
            WaveDifficultyCalculator.GetEnemyCount(waveNumber, _settings.Difficulty),
            Is.EqualTo(expectedCount));
    }

    [Test]
    public void WavesOneThroughThree_ContainOnlyNormalEnemies()
    {
        for (int waveNumber = 1; waveNumber <= 3; waveNumber++)
        {
            WavePlan plan = _generator.CreatePlan(waveNumber, 100 + waveNumber);

            Assert.That(plan.PatternType, Is.EqualTo(WavePatternType.NormalOnly));
            Assert.That(plan.EnemyCounts[WaveEnemyType.Normal], Is.EqualTo(plan.TotalEnemyCount));
            Assert.That(plan.EnemyCounts[WaveEnemyType.Ranged], Is.Zero);
            Assert.That(plan.EnemyCounts[WaveEnemyType.Tank], Is.Zero);
            Assert.That(plan.EnemyCounts[WaveEnemyType.Split], Is.Zero);
            Assert.That(plan.EnemyCounts[WaveEnemyType.Healer], Is.Zero);
        }
    }

    [Test]
    public void PhaseGuarantees_AreAppliedBeforePatternAllocation()
    {
        WavePlan wave4 = _generator.CreatePlan(4, 4);
        WavePlan wave7 = _generator.CreatePlan(7, 7);
        WavePlan wave8 = _generator.CreatePlan(8, 8);
        WavePlan wave9 = _generator.CreatePlan(9, 9);

        Assert.That(wave4.EnemyCounts[WaveEnemyType.Ranged], Is.GreaterThanOrEqualTo(1));
        Assert.That(wave4.EnemyCounts[WaveEnemyType.Healer], Is.GreaterThanOrEqualTo(1));
        Assert.That(wave7.EnemyCounts[WaveEnemyType.Tank], Is.GreaterThanOrEqualTo(1));
        Assert.That(wave8.EnemyCounts[WaveEnemyType.Healer], Is.GreaterThanOrEqualTo(1));
        Assert.That(wave9.EnemyCounts[WaveEnemyType.Split], Is.GreaterThanOrEqualTo(1));
        Assert.That(wave9.EnemyCounts[WaveEnemyType.Healer], Is.GreaterThanOrEqualTo(1));

        for (int waveNumber = 5; waveNumber <= 7; waveNumber++)
        {
            WavePlan plan = _generator.CreatePlan(waveNumber, waveNumber);
            Assert.That(plan.EnemyCounts[WaveEnemyType.Healer], Is.Zero);
        }
    }

    [Test]
    public void EveryPlan_PreservesExactTargetCountAndScheduleCount()
    {
        for (int waveNumber = 1; waveNumber <= 50; waveNumber++)
        {
            WavePlan plan = _generator.CreatePlan(waveNumber, 5000 + waveNumber);
            int allocatedCount = 0;

            foreach (KeyValuePair<WaveEnemyType, int> pair in plan.EnemyCounts)
                allocatedCount += pair.Value;

            Assert.That(allocatedCount, Is.EqualTo(plan.TotalEnemyCount), $"Wave {waveNumber}");
            Assert.That(
                plan.TotalEnemyCount,
                Is.EqualTo(WaveDifficultyCalculator.GetEnemyCount(waveNumber, _settings.Difficulty)),
                $"Wave {waveNumber}");
        }
    }

    [Test]
    public void SameSeed_ProducesTheSamePatternCountsAndSchedule()
    {
        WavePlan first = _generator.CreatePlan(12, 123456);
        WavePlan second = _generator.CreatePlan(12, 123456);

        Assert.That(second.PatternType, Is.EqualTo(first.PatternType));

        foreach (KeyValuePair<WaveEnemyType, int> pair in first.EnemyCounts)
            Assert.That(second.EnemyCounts[pair.Key], Is.EqualTo(pair.Value));

        Assert.That(second.SpawnInstructions.Count, Is.EqualTo(first.SpawnInstructions.Count));

        for (int i = 0; i < first.SpawnInstructions.Count; i++)
        {
            Assert.That(second.SpawnInstructions[i].EnemyType, Is.EqualTo(first.SpawnInstructions[i].EnemyType));
            Assert.That(second.SpawnInstructions[i].SpawnTime, Is.EqualTo(first.SpawnInstructions[i].SpawnTime));
        }
    }

    [Test]
    public void ZeroWeightEnemy_OnlyReceivesItsRequiredMinimum()
    {
        WavePlan tankPatternPlan = null;

        for (int seed = 0; seed < 1000; seed++)
        {
            WavePlan candidate = _generator.CreatePlan(9, seed);

            if (candidate.PatternType != WavePatternType.TankFrontline)
                continue;

            tankPatternPlan = candidate;
            break;
        }

        Assert.That(tankPatternPlan, Is.Not.Null);
        Assert.That(tankPatternPlan.EnemyCounts[WaveEnemyType.Split], Is.EqualTo(1));
    }

    [Test]
    public void DifficultyMultipliers_UseWaveAndIntegerDamageSteps()
    {
        EnemyRuntimeModifiers wave1 = WaveDifficultyCalculator.GetRuntimeModifiers(1, _settings.Difficulty);
        EnemyRuntimeModifiers wave2 = WaveDifficultyCalculator.GetRuntimeModifiers(2, _settings.Difficulty);
        EnemyRuntimeModifiers wave3 = WaveDifficultyCalculator.GetRuntimeModifiers(3, _settings.Difficulty);

        Assert.That(wave1.HpMultiplier, Is.EqualTo(1f).Within(0.0001f));
        Assert.That(wave2.HpMultiplier, Is.EqualTo(1.1f).Within(0.0001f));
        Assert.That(wave3.HpMultiplier, Is.EqualTo(1.21f).Within(0.0001f));
        Assert.That(wave1.DamageMultiplier, Is.EqualTo(1f).Within(0.0001f));
        Assert.That(wave2.DamageMultiplier, Is.EqualTo(1f).Within(0.0001f));
        Assert.That(wave3.DamageMultiplier, Is.EqualTo(1.07f).Within(0.0001f));
    }

    [Test]
    public void WaveOne_SpawnsFromFirstToLastOverFifteenSeconds()
    {
        WavePlan plan = _generator.CreatePlan(1, 1);

        Assert.That(plan.SpawnInstructions[0].SpawnTime, Is.Zero);
        Assert.That(plan.SpawnDuration, Is.EqualTo(15f).Within(0.0001f));
    }

    [TestCase(1)]
    [TestCase(5)]
    [TestCase(10)]
    [TestCase(20)]
    public void SpawnSchedule_UsesWaveScaledIntervalAndExpectedCompletionTime(int waveNumber)
    {
        WavePlan plan = _generator.CreatePlan(waveNumber, waveNumber);
        float expectedInterval = System.Math.Max(
            _settings.SpawnSchedule.MinimumSpawnInterval,
            _settings.SpawnSchedule.SpawnInterval * (float)System.Math.Pow(0.96f, waveNumber - 1));
        float actualInterval = plan.SpawnInstructions[1].SpawnTime -
            plan.SpawnInstructions[0].SpawnTime;
        float expectedCompletionTime = expectedInterval * (plan.TotalEnemyCount - 1);

        Assert.That(
            _settings.SpawnSchedule.SpawnIntervalMultiplierPerWave,
            Is.EqualTo(0.96f).Within(0.0001f));
        Assert.That(
            _settings.SpawnSchedule.MinimumSpawnInterval,
            Is.EqualTo(0.35f).Within(0.0001f));
        Assert.That(actualInterval, Is.EqualTo(expectedInterval).Within(0.0001f));
        Assert.That(plan.SpawnDuration, Is.EqualTo(expectedCompletionTime).Within(0.0001f));
    }

    [Test]
    public void SpawnSchedule_DoesNotFallBelowConfiguredMinimumInterval()
    {
        WavePlan plan = _generator.CreatePlan(100, 100);
        float actualInterval = plan.SpawnInstructions[1].SpawnTime -
            plan.SpawnInstructions[0].SpawnTime;

        Assert.That(
            actualInterval,
            Is.EqualTo(_settings.SpawnSchedule.MinimumSpawnInterval).Within(0.0001f));
    }

    [Test]
    public void MaximumCounts_AreRespectedAndImpossibleConfigurationThrows()
    {
        WaveGenerationSettings clone = Object.Instantiate(_settings);

        try
        {
            var serializedSettings = new SerializedObject(clone);
            SerializedProperty phases = serializedSettings.FindProperty("_enemyPhases");

            for (int i = 0; i < phases.arraySize; i++)
            {
                SerializedProperty phase = phases.GetArrayElementAtIndex(i);
                phase.FindPropertyRelative("_maximumCount").intValue = 1;
            }

            serializedSettings.ApplyModifiedPropertiesWithoutUndo();
            var impossibleGenerator = new WavePlanGenerator(clone);

            Assert.Throws<WaveConfigurationException>(() => impossibleGenerator.CreatePlan(4, 1));
        }
        finally
        {
            Object.DestroyImmediate(clone);
        }
    }

    [Test]
    public void MaximumCount_PreventsFurtherWeightedAllocation()
    {
        WaveGenerationSettings clone = Object.Instantiate(_settings);

        try
        {
            var serializedSettings = new SerializedObject(clone);
            SerializedProperty phases = serializedSettings.FindProperty("_enemyPhases");

            for (int i = 0; i < phases.arraySize; i++)
            {
                SerializedProperty phase = phases.GetArrayElementAtIndex(i);

                if (phase.FindPropertyRelative("_enemyType").enumValueIndex == (int)WaveEnemyType.Tank)
                    phase.FindPropertyRelative("_maximumCount").intValue = 1;
            }

            serializedSettings.ApplyModifiedPropertiesWithoutUndo();
            var cappedGenerator = new WavePlanGenerator(clone);
            WavePlan tankPatternPlan = null;

            for (int seed = 0; seed < 1000; seed++)
            {
                WavePlan candidate = cappedGenerator.CreatePlan(12, seed);

                if (candidate.PatternType != WavePatternType.TankFrontline)
                    continue;

                tankPatternPlan = candidate;
                break;
            }

            Assert.That(tankPatternPlan, Is.Not.Null);
            Assert.That(tankPatternPlan.EnemyCounts[WaveEnemyType.Tank], Is.EqualTo(1));
            Assert.That(tankPatternPlan.TotalEnemyCount, Is.EqualTo(42));
        }
        finally
        {
            Object.DestroyImmediate(clone);
        }
    }
}
