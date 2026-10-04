using UnityEngine;

public class Tower : MonoBehaviour
{
    private SpriteRenderer _spriteRenderer;

    public TowerHealth Health { get; private set; }
    public TowerStats Stats { get; private set; }
    public TowerAttack Attack { get; private set; }

    private void Awake()
    {
        _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    public void Initialize(TowerHealth health, TowerStats stats, TowerAttack attack, Sprite sprite)
    {
        Health = health;
        Stats = stats;
        Attack = attack;

        if (_spriteRenderer != null)
            _spriteRenderer.sprite = sprite;
    }
}