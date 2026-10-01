using System;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class Enemy : MonoBehaviour
{
    public EnemyHealth Health { get; private set; }
    public EnemyStats Stats { get; private set; }
    public IEnemyAttack Attack { get; private set; }

    public event Action<Enemy> Died;

    private EnemyController _controller;
    private Transform _moveTarget;
    private SpriteRenderer _spriteRenderer;

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void Initialize(EnemyHealth health, EnemyStats stats, EnemyController controller, IEnemyAttack attack, Transform moveTarget, Sprite sprite)
    {
        if (Health != null)
            Health.Died -= OnDied;

        Health = health;
        Stats = stats;
        Attack = attack;
        _controller = controller;
        _moveTarget = moveTarget;

        _spriteRenderer.sprite = sprite;

        Health.Died += OnDied;
    }

    private void Update()
    {
        if (_controller == null || _moveTarget == null)
            return;

        _controller.EnemyUpdate(transform, _moveTarget.position, Time.deltaTime);
    }

    private void OnDied()
    {
        Died?.Invoke(this);
    }

    private void OnDestroy()
    {
        if (Health != null)
            Health.Died -= OnDied;
    }
}