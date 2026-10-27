
using UnityEngine;

[CreateAssetMenu(fileName = "EnemyConfig", menuName = "Scriptable Objects/EnemyConfig")]
public class EnemyConfig : ScriptableObject
{
    [Header("Role")]
    [SerializeField] private EnemyRole _role = EnemyRole.Combat;
    [Tooltip("Seconds used by support enemies when leaving after combat enemies are gone.")]
    [SerializeField, Min(0.01f)] private float _retreatDuration = 0.35f;

    [Header("Stats")]
    [SerializeField] private float _maxHp;
    [SerializeField] private float _moveSpeed;
    [SerializeField] private float _attackDamage;
    [SerializeField] private float _attackRange;
    [SerializeField] private float _attackCooldown;
    [SerializeField, Min(0)] private int _goldReward;
    [SerializeField] private Sprite _sprite;

    [SerializeReference] private EnemyMovementDefinition _movementDefinition;
    [SerializeReference] private EnemyAttackDefinition _attackDefinition;
    [SerializeReference] private EnemyAbilityDefinition _abilityDefinition;
    [SerializeReference] private EnemyDeathBehaviorDefinition _deathBehaviorDefinition;

    public EnemyRole Role => _role;
    public float RetreatDuration => Mathf.Max(0.01f, _retreatDuration);
    public float MaxHp => _maxHp;
    public float MoveSpeed => _moveSpeed;
    public float AttackDamage => _attackDamage;
    public float AttackRange => _attackRange;
    public float AttackCooldown => _attackCooldown;
    public int GoldReward => _goldReward;
    public Sprite Sprite => _sprite;

    public EnemyMovementDefinition MovementDefinition => _movementDefinition;
    public EnemyAttackDefinition AttackDefinition => _attackDefinition;
    public EnemyAbilityDefinition AbilityDefinition => _abilityDefinition;
    public EnemyDeathBehaviorDefinition DeathBehaviorDefinition => _deathBehaviorDefinition;
    public bool CanReceiveHealing => _abilityDefinition == null || _abilityDefinition.CanReceiveHealing;
}
