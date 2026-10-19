using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RewardData", menuName = "Scriptable Objects/RewardData")]
public class RewardData : ScriptableObject
{
    [SerializeField] private string _rewardName;
    [SerializeField] private string _description;
    [SerializeField] private Sprite _icon;

    [SerializeReference, SubclassSelector(nameof(_rewardDefinitions))]
    private RewardDefinition _rewardDefinition;

    [SerializeReference, HideInInspector] private List<RewardDefinition> _rewardDefinitions = new();

    public string RewardName => _rewardName;
    public string Description => _description;
    public Sprite Icon => _icon;
    public RewardDefinition RewardDefinition => _rewardDefinition;
}