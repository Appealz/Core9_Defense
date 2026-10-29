using NUnit.Framework;
using UnityEditor;
using UnityEngine;

public sealed class RewardProbabilityDistributionTests
{
    [Test]
    public void EditingOneRarity_PreservesItsValueAndRedistributesTheRestProportionally()
    {
        float[] result = RewardProbabilityDistribution.Redistribute(
            new[] { 40f, 30f, 20f, 10f },
            0,
            60f);

        Assert.That(result[0], Is.EqualTo(60f).Within(0.0001f));
        Assert.That(result[1], Is.EqualTo(20f).Within(0.0001f));
        Assert.That(result[2], Is.EqualTo(13.333333f).Within(0.0001f));
        Assert.That(result[3], Is.EqualTo(6.666667f).Within(0.0001f));
        AssertTotalIsExactlyOneHundred(result);
    }

    [Test]
    public void EditingToOneHundred_SetsAllOtherRaritiesToZero()
    {
        float[] result = RewardProbabilityDistribution.Redistribute(
            new[] { 25f, 25f, 25f, 25f },
            2,
            100f);

        Assert.That(result, Is.EqualTo(new[] { 0f, 0f, 100f, 0f }));
        AssertTotalIsExactlyOneHundred(result);
    }

    [Test]
    public void ZeroOtherTotal_DistributesRemainingProbabilityEqually()
    {
        float[] result = RewardProbabilityDistribution.Redistribute(
            new[] { 100f, 0f, 0f, 0f },
            0,
            40f);

        Assert.That(result[0], Is.EqualTo(40f).Within(0.0001f));
        Assert.That(result[1], Is.EqualTo(20f).Within(0.0001f));
        Assert.That(result[2], Is.EqualTo(20f).Within(0.0001f));
        Assert.That(result[3], Is.EqualTo(20f).Within(0.0001f));
        AssertTotalIsExactlyOneHundred(result);
    }

    [TestCase(-20f, 0f)]
    [TestCase(120f, 100f)]
    public void EditedValue_IsClampedToValidRange(float editedValue, float expectedValue)
    {
        float[] result = RewardProbabilityDistribution.Redistribute(
            new[] { 40f, 30f, 20f, 10f },
            1,
            editedValue);

        Assert.That(result[1], Is.EqualTo(expectedValue).Within(0.0001f));
        AssertTotalIsExactlyOneHundred(result);
    }

    [Test]
    public void RepeatedEdits_DoNotAccumulateTotalProbabilityError()
    {
        float[] result = { 47.61905f, 30.95238f, 16.66667f, 4.7619f };

        for (int i = 0; i < 1000; i++)
        {
            int changedIndex = i % result.Length;
            float editedValue = (i * 17.123f) % 100f;
            result = RewardProbabilityDistribution.Redistribute(result, changedIndex, editedValue);
            AssertTotalIsExactlyOneHundred(result);
        }
    }

    [Test]
    public void SerializedProbabilityChanges_SupportUndoAndRedo()
    {
        GameStartData gameStartData = ScriptableObject.CreateInstance<GameStartData>();

        try
        {
            Undo.ClearAll();
            SerializedObject serializedObject = new SerializedObject(gameStartData);
            SerializedProperty settings = serializedObject.FindProperty("_rewardSelectionSettings");
            SerializedProperty bronze = settings.FindPropertyRelative("_bronzeProbability");
            SerializedProperty silver = settings.FindPropertyRelative("_silverProbability");
            SerializedProperty gold = settings.FindPropertyRelative("_goldProbability");
            SerializedProperty platinum = settings.FindPropertyRelative("_platinumProbability");
            float originalBronze = bronze.floatValue;

            float[] redistributed = RewardProbabilityDistribution.Redistribute(
                new[] { bronze.floatValue, silver.floatValue, gold.floatValue, platinum.floatValue },
                0,
                60f);

            bronze.floatValue = redistributed[0];
            silver.floatValue = redistributed[1];
            gold.floatValue = redistributed[2];
            platinum.floatValue = redistributed[3];
            Assert.That(serializedObject.ApplyModifiedProperties(), Is.True);
            Assert.That(gameStartData.RewardSelectionSettings.GetRarityProbability(RewardRarity.Bronze),
                Is.EqualTo(60f).Within(0.0001f));

            Undo.PerformUndo();
            serializedObject.Update();
            Assert.That(bronze.floatValue, Is.EqualTo(originalBronze).Within(0.0001f));

            Undo.PerformRedo();
            serializedObject.Update();
            Assert.That(bronze.floatValue, Is.EqualTo(60f).Within(0.0001f));
        }
        finally
        {
            Undo.ClearAll();
            Object.DestroyImmediate(gameStartData);
        }
    }

    private static void AssertTotalIsExactlyOneHundred(float[] values)
    {
        float total = 0f;

        for (int i = 0; i < values.Length; i++)
        {
            Assert.That(values[i], Is.InRange(0f, 100f));
            total += values[i];
        }

        Assert.That(total, Is.EqualTo(100f).Within(0.0001f));
    }
}
