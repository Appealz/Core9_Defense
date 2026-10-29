using System;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(RewardSelectionSettings))]
public sealed class RewardSelectionSettingsDrawer : PropertyDrawer
{
    private const int ExpandedLineCount = 8;
    private static readonly string[] ProbabilityPropertyNames =
    {
        "_bronzeProbability",
        "_silverProbability",
        "_goldProbability",
        "_platinumProbability"
    };

    private static readonly GUIContent[] ProbabilityLabels =
    {
        new("Bronze"),
        new("Silver"),
        new("Gold"),
        new("Platinum")
    };

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        Rect lineRect = GetLineRect(position, 0);
        property.isExpanded = EditorGUI.Foldout(lineRect, property.isExpanded, label, true);

        if (!property.isExpanded)
        {
            EditorGUI.EndProperty();
            return;
        }

        EditorGUI.indentLevel++;

        SerializedProperty cardCount = property.FindPropertyRelative("_cardCount");
        SerializedProperty minimumNewTowerCards = property.FindPropertyRelative("_minimumNewTowerCards");

        EditorGUI.PropertyField(GetLineRect(position, 1), cardCount);
        EditorGUI.PropertyField(GetLineRect(position, 2), minimumNewTowerCards);
        EditorGUI.LabelField(GetLineRect(position, 3), "Rarity Probabilities (%)", EditorStyles.boldLabel);

        SerializedProperty[] probabilityProperties = FindProbabilityProperties(property);
        float[] probabilities = ReadProbabilities(probabilityProperties);

        for (int i = 0; i < probabilityProperties.Length; i++)
        {
            SerializedProperty probabilityProperty = probabilityProperties[i];
            EditorGUI.showMixedValue = probabilityProperty.hasMultipleDifferentValues;
            EditorGUI.BeginChangeCheck();
            float editedValue = EditorGUI.Slider(
                GetLineRect(position, i + 4),
                ProbabilityLabels[i],
                probabilityProperty.floatValue,
                0f,
                100f);

            if (!EditorGUI.EndChangeCheck())
                continue;

            probabilities = RewardProbabilityDistribution.Redistribute(probabilities, i, editedValue);
            WriteProbabilities(probabilityProperties, probabilities);
        }

        EditorGUI.showMixedValue = false;
        EditorGUI.indentLevel--;
        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        int lineCount = property.isExpanded ? ExpandedLineCount : 1;
        return lineCount * EditorGUIUtility.singleLineHeight +
            (lineCount - 1) * EditorGUIUtility.standardVerticalSpacing;
    }

    private static Rect GetLineRect(Rect position, int lineIndex)
    {
        float lineHeight = EditorGUIUtility.singleLineHeight;
        float y = position.y + lineIndex * (lineHeight + EditorGUIUtility.standardVerticalSpacing);
        return new Rect(position.x, y, position.width, lineHeight);
    }

    private static SerializedProperty[] FindProbabilityProperties(SerializedProperty property)
    {
        SerializedProperty[] properties = new SerializedProperty[ProbabilityPropertyNames.Length];

        for (int i = 0; i < properties.Length; i++)
            properties[i] = property.FindPropertyRelative(ProbabilityPropertyNames[i]);

        return properties;
    }

    private static float[] ReadProbabilities(SerializedProperty[] properties)
    {
        float[] values = new float[properties.Length];

        for (int i = 0; i < properties.Length; i++)
            values[i] = properties[i].floatValue;

        return values;
    }

    private static void WriteProbabilities(SerializedProperty[] properties, float[] values)
    {
        for (int i = 0; i < properties.Length; i++)
            properties[i].floatValue = values[i];
    }
}

internal static class RewardProbabilityDistribution
{
    private const float TotalProbability = 100f;
    private const float Epsilon = 0.000001f;

    public static float[] Redistribute(float[] currentValues, int changedIndex, float editedValue)
    {
        if (currentValues == null)
            throw new ArgumentNullException(nameof(currentValues));

        if (currentValues.Length != 4)
            throw new ArgumentException("Exactly four rarity probabilities are required.", nameof(currentValues));

        if (changedIndex < 0 || changedIndex >= currentValues.Length)
            throw new ArgumentOutOfRangeException(nameof(changedIndex));

        float changedValue = SanitizeProbability(editedValue);
        float remainingTotal = TotalProbability - changedValue;
        float[] result = new float[currentValues.Length];
        result[changedIndex] = changedValue;

        double otherTotal = 0d;

        for (int i = 0; i < currentValues.Length; i++)
        {
            if (i != changedIndex)
                otherTotal += SanitizeProbability(currentValues[i]);
        }

        int correctionIndex = FindCorrectionIndex(changedIndex, currentValues.Length);

        if (otherTotal > Epsilon)
        {
            double scale = remainingTotal / otherTotal;

            for (int i = 0; i < currentValues.Length; i++)
            {
                if (i != changedIndex && i != correctionIndex)
                    result[i] = Mathf.Clamp((float)(SanitizeProbability(currentValues[i]) * scale), 0f, remainingTotal);
            }
        }
        else
        {
            float equalShare = remainingTotal / (currentValues.Length - 1);

            for (int i = 0; i < currentValues.Length; i++)
            {
                if (i != changedIndex && i != correctionIndex)
                    result[i] = equalShare;
            }
        }

        float allocatedWithoutCorrection = changedValue;

        for (int i = 0; i < result.Length; i++)
        {
            if (i != changedIndex && i != correctionIndex)
                allocatedWithoutCorrection += result[i];
        }

        result[correctionIndex] = Mathf.Clamp(
            TotalProbability - allocatedWithoutCorrection,
            0f,
            remainingTotal);

        float residual = TotalProbability - Sum(result);
        result[correctionIndex] = Mathf.Clamp(result[correctionIndex] + residual, 0f, TotalProbability);
        return result;
    }

    private static float SanitizeProbability(float value)
    {
        return float.IsNaN(value) || float.IsInfinity(value)
            ? 0f
            : Mathf.Clamp(value, 0f, TotalProbability);
    }

    private static int FindCorrectionIndex(int changedIndex, int count)
    {
        for (int i = count - 1; i >= 0; i--)
        {
            if (i != changedIndex)
                return i;
        }

        throw new InvalidOperationException("No probability is available for residual correction.");
    }

    private static float Sum(float[] values)
    {
        float total = 0f;

        for (int i = 0; i < values.Length; i++)
            total += values[i];

        return total;
    }
}
