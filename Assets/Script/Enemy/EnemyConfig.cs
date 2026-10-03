using UnityEngine;

[CreateAssetMenu(fileName = "EnemyConfig", menuName = "Scriptable Objects/EnemyConfig")]
public class EnemyConfig : ScriptableObject
{
    [SerializeField] private float _maxHp;
    [SerializeField] private float _moveSpeed;
    [SerializeField] private float _attackDamage;
    [SerializeField] private float _attackRange;
    [SerializeField] private float _attackCooldown;
    [SerializeField] private Sprite _sprite;

    [SerializeReference] private EnemyMovementDefinition _movementDefinition;
    [SerializeReference] private EnemyAttackDefinition _attackDefinition;

    public float MaxHp => _maxHp;
    public float MoveSpeed => _moveSpeed;
    public float AttackDamage => _attackDamage;
    public float AttackRange => _attackRange;
    public float AttackCooldown => _attackCooldown;
    public Sprite Sprite => _sprite;

    public EnemyMovementDefinition MovementDefinition => _movementDefinition;
    public EnemyAttackDefinition AttackDefinition => _attackDefinition;
}