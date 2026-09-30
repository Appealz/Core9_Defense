using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TowerPartData", menuName = "Scriptable Objects/TowerPartData")]
public class TowerPartData : ScriptableObject
{
    [SerializeField] private int _maxHp;
    [SerializeField] private float _attackDamage;
    [SerializeField] private float _attackSpeed;
    [SerializeField] private float _attackRange;
    [SerializeField] private float _attackRate;
    [SerializeField] private Sprite _sprite;
    [SerializeReference] private TowerAttackDefinition _attackDefinition;
    [SerializeField] private string _prefabKey;

    

    [SerializeReference, HideInInspector]
    private List<TowerAttackDefinition> _attackDefinitions = new();
        
    public int MaxHp => _maxHp;
    public float AttackDamage => _attackDamage;
    public float AttackSpeed => _attackSpeed;
    public float AttackRange => _attackRange;
    public float AttackRate => _attackRate;
    public Sprite Sprite => _sprite;
    public TowerAttackDefinition AttackDefinition => _attackDefinition;
    public string PrefabKey => _prefabKey;
}
