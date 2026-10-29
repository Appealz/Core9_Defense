using System;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
public sealed class RewardSelectionSettings
{
    [Header("Card Selection")]
    [Tooltip("Number of unique cards offered after a wave.")]
    [SerializeField, Min(1)] private int _cardCount = 3;
    [Tooltip("Minimum NewTower cards when at least this many valid candidates exist.")]
    [SerializeField, Min(0)] private int _minimumNewTowerCards = 1;

    [Header("Rarity Probabilities (%)")]
    [Tooltip("Rarity probabilities must add up to 100%.")]
    [FormerlySerializedAs("_bronzeWeight")]
    [SerializeField, Range(0f, 100f)] private float _bronzeProbability = 47.61905f;
    [FormerlySerializedAs("_silverWeight")]
    [SerializeField, Range(0f, 100f)] private float _silverProbability = 30.95238f;
    [FormerlySerializedAs("_goldWeight")]
    [SerializeField, Range(0f, 100f)] private float _goldProbability = 16.66667f;
    [FormerlySerializedAs("_platinumWeight")]
    [SerializeField, Range(0f, 100f)] private float _platinumProbability = 4.7619f;

    public int CardCount => Mathf.Max(1, _cardCount);
    public int MinimumNewTowerCards => Mathf.Clamp(_minimumNewTowerCards, 0, CardCount);

    public float GetRarityProbability(RewardRarity rarity)
    {
        return rarity switch
        {
            RewardRarity.Bronze => Mathf.Max(0f, _bronzeProbability),
            RewardRarity.Silver => Mathf.Max(0f, _silverProbability),
            RewardRarity.Gold => Mathf.Max(0f, _goldProbability),
            RewardRarity.Platinum => Mathf.Max(0f, _platinumProbability),
            _ => 0f
        };
    }

    public bool TryValidate(out string error)
    {
        float[] probabilities =
        {
            _bronzeProbability,
            _silverProbability,
            _goldProbability,
            _platinumProbability
        };

        float total = 0f;

        for (int i = 0; i < probabilities.Length; i++)
        {
            float probability = probabilities[i];

            if (float.IsNaN(probability) || float.IsInfinity(probability) ||
                probability < 0f || probability > 100f)
            {
                error = "Reward rarity probabilities must each be between 0 and 100.";
                return false;
            }

            total += probability;
        }

        if (Mathf.Abs(total - 100f) > 0.01f)
        {
            error = $"Reward rarity probabilities must add up to 100%. Current total: {total:0.###}%";
            return false;
        }

        error = null;
        return true;
    }
}
