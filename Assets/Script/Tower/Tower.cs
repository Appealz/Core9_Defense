
using UnityEngine;

[RequireComponent(typeof(TowerHitFlash))]
public class Tower : MonoBehaviour, IDamageable
{
    private SpriteRenderer _spriteRenderer;
    private TowerHitFlash _hitFlash;
    private readonly TowerSlotEffectRuntime _slotEffectRuntime = new();
    private ITowerAttackEffect _attackEffect;

    public TowerHealth Health { get; private set; }
    public TowerStats Stats { get; private set; }
    public TowerAttack Attack { get; private set; }
    public TowerType TowerType { get; private set; }

    public bool IsAlive => Health != null && Health.IsAlive;
    public Vector3 Position => transform.position;

    private void Awake()
    {
        _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        _hitFlash = GetComponent<TowerHitFlash>();
    }

    private void Update()
    {
        Attack?.Update(transform.position, Time.deltaTime);
        _slotEffectRuntime.Update(Time.deltaTime);
    }

    public void Initialize(
        TowerHealth health,
        TowerStats stats,
        TowerAttack attack,
        Sprite sprite,
        TowerType towerType,
        ITowerAttackEffect attackEffect)
    {
        Health = health;
        Stats = stats;
        Attack = attack;
        TowerType = towerType;
        _attackEffect = attackEffect;

        if (_spriteRenderer != null)
            _spriteRenderer.sprite = sprite;

        if (_hitFlash == null)
            throw new MissingComponentException("TowerHitFlash 컴포넌트가 없습니다.");

        _hitFlash.Initialize(health);
        RefreshAttackEffect();
    }

    public void TakeDamage(float damage)
    {
        Health?.TakeDamage(damage);
    }

    public void BindSlotEffects(System.Collections.Generic.IReadOnlyList<CoreSlotEffect> effects)
    {
        _slotEffectRuntime.Bind(this, effects);
    }

    public void RefreshAttackEffect()
    {
        _attackEffect?.Show(transform, Stats);
    }

    private void OnDisable()
    {
        _slotEffectRuntime.Clear();
    }
}
