
using UnityEngine;

[CreateAssetMenu(fileName = "EnemyConfig", menuName = "Scriptable Objects/EnemyConfig")]
public class EnemyConfig : ScriptableObject
{
    [Header("Role")]
    [SerializeField] private EnemyRole _role = EnemyRole.Combat;
    [Tooltip("Seconds a support enemy must avoid damage before retreating.")]
    [SerializeField, Min(0f)] private float _retreatDelay = 5f;
    [Tooltip("World units per second while a support enemy retreats away from the core.")]
    [SerializeField, Min(0.01f)] private float _retreatMoveSpeed = 8f;
    [Tooltip("Additional distance beyond the camera bounds before retreat completes.")]
    [SerializeField, Min(0f)] private float _retreatExitMargin = 1f;

    [Header("Stats")]
    [SerializeField] private float _maxHp;
    [SerializeField] private float _moveSpeed;
    [SerializeField] private float _attackDamage;
    [SerializeField] private float _attackRange;
    [SerializeField] private float _attackCooldown;
    [SerializeField, Min(0)] private int _goldReward;
    [SerializeField, Min(0)] private int _experienceReward = 1;
    [SerializeField] private Sprite _sprite;

    [SerializeReference] private EnemyMovementDefinition _movementDefinition;
    [SerializeReference] private EnemyAttackDefinition _attackDefinition;
    [SerializeReference] private EnemyAbilityDefinition _abilityDefinition;
    [SerializeReference] private EnemyDeathBehaviorDefinition _deathBehaviorDefinition;

    public EnemyRole Role => _role;
    public float RetreatDelay => Mathf.Max(0f, _retreatDelay);
    public float RetreatMoveSpeed => Mathf.Max(0.01f, _retreatMoveSpeed);
    public float RetreatExitMargin => Mathf.Max(0f, _retreatExitMargin);
    public float MaxHp => _maxHp;
    public float MoveSpeed => _moveSpeed;
    public float AttackDamage => _attackDamage;
    public float AttackRange => _attackRange;
    public float AttackCooldown => _attackCooldown;
    public int GoldReward => _goldReward;
    public int ExperienceReward => _experienceReward;
    public Sprite Sprite => _sprite;

    public EnemyMovementDefinition MovementDefinition => _movementDefinition;
    public EnemyAttackDefinition AttackDefinition => _attackDefinition;
    public EnemyAbilityDefinition AbilityDefinition => _abilityDefinition;
    public EnemyDeathBehaviorDefinition DeathBehaviorDefinition => _deathBehaviorDefinition;
    public bool CanReceiveHealing => _abilityDefinition == null || _abilityDefinition.CanReceiveHealing;
}
