using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TowerPartData", menuName = "Scriptable Objects/TowerPartData")]
public class TowerPartData : ScriptableObject
{
    [SerializeField] private string _towerName;
    [SerializeField] private int _maxHp;
    [SerializeField] private float _attackDamage;
    [SerializeField] private float _attackRange;
    [SerializeField] private float _attackRate;
    [SerializeField] private Sprite _sprite;

    [SerializeReference] private TowerAttackDefinition _attackDefinition;
    [SerializeReference] private FireModeDefinition _fireModeDefinition;

    [SerializeReference, HideInInspector]
    private List<TowerAttackDefinition> _attackDefinitions = new();

    [SerializeReference, HideInInspector]
    private List<FireModeDefinition> _fireModeDefinitions = new();

    public string TowerName => _towerName;
    public int MaxHp => _maxHp;
    public float AttackDamage => _attackDamage;
    public float AttackRange => _attackRange;
    public float AttackRate => _attackRate;
    public Sprite Sprite => _sprite;
    public TowerAttackDefinition AttackDefinition => _attackDefinition;
    public FireModeDefinition FireModeDefinition => _fireModeDefinition;
}