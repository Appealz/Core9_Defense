using System;
using UnityEngine;

[Serializable]
public sealed class RewardSelectionSettings
{
    [Header("Card Selection")]
    [Tooltip("Number of unique cards offered after a wave.")]
    [SerializeField, Min(1)] private int _cardCount = 3;
    [Tooltip("Minimum NewTower cards when at least this many valid candidates exist.")]
    [SerializeField, Min(0)] private int _minimumNewTowerCards = 1;

    [Header("Rarity Weight Multipliers")]
    [Tooltip("Final weight = reward selection weight x rarity weight.")]
    [SerializeField, Min(0f)] private float _bronzeWeight = 1f;
    [SerializeField, Min(0f)] private float _silverWeight = 1f;
    [SerializeField, Min(0f)] private float _goldWeight = 1f;
    [SerializeField, Min(0f)] private float _platinumWeight = 1f;

    public int CardCount => Mathf.Max(1, _cardCount);
    public int MinimumNewTowerCards => Mathf.Clamp(_minimumNewTowerCards, 0, CardCount);

    public float GetRarityWeight(RewardRarity rarity)
    {
        return rarity switch
        {
            RewardRarity.Bronze => Mathf.Max(0f, _bronzeWeight),
            RewardRarity.Silver => Mathf.Max(0f, _silverWeight),
            RewardRarity.Gold => Mathf.Max(0f, _goldWeight),
            RewardRarity.Platinum => Mathf.Max(0f, _platinumWeight),
            _ => 0f
        };
    }
}
