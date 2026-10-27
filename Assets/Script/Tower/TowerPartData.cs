using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TowerPartData", menuName = "Scriptable Objects/TowerPartData")]
public class TowerPartData : ScriptableObject
{
    [SerializeField] private TowerType _towerType;
    [SerializeField] private string _towerName;
    [SerializeField] private int _maxHp;
    [SerializeField] private float _attackDamage;
    [SerializeField] private float _attackRange;
    [Tooltip("Seconds between attack sequences before attack-speed modifiers.")]
    [SerializeField, Min(0f)] private float _attackRate;
    [Header("Combat Limits")]
    [Tooltip("Base critical chance in 0-1 format.")]
    [SerializeField, Range(0f, 1f)] private float _criticalChance;
    [Tooltip("Damage multiplier applied when one attack roll is critical.")]
    [SerializeField, Min(1f)] private float _criticalDamageMultiplier = 2f;
    [Tooltip("Maximum final critical chance after all modifiers.")]
    [SerializeField, Range(0f, 1f)] private float _criticalChanceCap = 1f;
    [Tooltip("Smallest allowed interval between attack sequences in seconds.")]
    [SerializeField, Min(0.01f)] private float _minimumAttackInterval = 0.05f;
    [SerializeField] private Sprite _sprite;

    [SerializeReference, SubclassSelector(nameof(_attackDefinitions))]
    private TowerAttackDefinition _attackDefinition;

    [SerializeReference, SubclassSelector(nameof(_fireModeDefinitions))]
    private FireModeDefinition _fireModeDefinition;

    [SerializeReference, HideInInspector] private List<TowerAttackDefinition> _attackDefinitions = new();
    [SerializeReference, HideInInspector] private List<FireModeDefinition> _fireModeDefinitions = new();

    public TowerType TowerType => _towerType;
    public string TowerName => _towerName;
    public int MaxHp => _maxHp;
    public float AttackDamage => _attackDamage;
    public float AttackRange => _attackRange;
    public float AttackRate => _attackRate;
    public float CriticalChance => _criticalChance;
    public float CriticalDamageMultiplier => _criticalDamageMultiplier;
    public float CriticalChanceCap => _criticalChanceCap;
    public float MinimumAttackInterval => _minimumAttackInterval;
    public Sprite Sprite => _sprite;
    public TowerAttackDefinition AttackDefinition => _attackDefinition;
    public FireModeDefinition FireModeDefinition => _fireModeDefinition;
}
