using System;
using UnityEngine;

[Serializable]
public sealed class RewardRarityColorPalette
{
    [SerializeField] private Color _bronze = new(0.55f, 0.32f, 0.16f, 1f);
    [SerializeField] private Color _silver = new(0.55f, 0.62f, 0.7f, 1f);
    [SerializeField] private Color _gold = new(0.92f, 0.67f, 0.12f, 1f);
    [SerializeField] private Color _platinum = new(0.3f, 0.82f, 0.9f, 1f);

    public Color GetColor(RewardRarity rarity)
    {
        return rarity switch
        {
            RewardRarity.Bronze => _bronze,
            RewardRarity.Silver => _silver,
            RewardRarity.Gold => _gold,
            RewardRarity.Platinum => _platinum,
            _ => Color.white
        };
    }
}
