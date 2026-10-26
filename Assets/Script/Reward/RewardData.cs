using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RewardData", menuName = "Scriptable Objects/RewardData")]
public class RewardData : ScriptableObject
{
    [Header("Card")]
    [SerializeField] private string _rewardName;
    [SerializeField] private string _description;
    [SerializeField] private Sprite _icon;

    [Header("Selection")]
    [Tooltip("Card rarity. Its global multiplier is configured in GameStartData.")]
    [SerializeField] private RewardRarity _rarity = RewardRarity.Bronze;
    [Tooltip("Used by selection guarantees and future reward-specific policies.")]
    [SerializeField] private RewardCategory _category = RewardCategory.Standard;
    [Tooltip("Individual draw weight. Zero prevents this reward from appearing.")]
    [SerializeField, Min(0f)] private float _selectionWeight = 1f;

    [Header("Behavior")]
    [SerializeReference, SubclassSelector(nameof(_rewardDefinitions))]
    private RewardDefinition _rewardDefinition;

    [SerializeReference, HideInInspector] private List<RewardDefinition> _rewardDefinitions = new();

    public string RewardName => _rewardName;
    public string Description => _description;
    public Sprite Icon => _icon;
    public RewardRarity Rarity => _rarity;
    public RewardCategory Category => _category;
    public float SelectionWeight => Mathf.Max(0f, _selectionWeight);
    public RewardDefinition RewardDefinition => _rewardDefinition;
}
