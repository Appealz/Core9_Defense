
using UnityEngine;

[RequireComponent(typeof(TowerHitFlash))]
public class Tower : MonoBehaviour, IDamageable
{
    private SpriteRenderer _spriteRenderer;
    private TowerHitFlash _hitFlash;

    public TowerHealth Health { get; private set; }
    public TowerStats Stats { get; private set; }
    public TowerAttack Attack { get; private set; }

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
    }

    public void Initialize(TowerHealth health, TowerStats stats, TowerAttack attack, Sprite sprite)
    {
        Health = health;
        Stats = stats;
        Attack = attack;

        if (_spriteRenderer != null)
            _spriteRenderer.sprite = sprite;

        if (_hitFlash == null)
            throw new MissingComponentException("TowerHitFlash 컴포넌트가 없습니다.");

        _hitFlash.Initialize(health);
    }

    public void TakeDamage(float damage)
    {
        Health?.TakeDamage(damage);
    }
}
