
using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class TowerHitFlash : MonoBehaviour
{
    [SerializeField, Min(1)] private int _flashCount = 3;
    [SerializeField, Min(0.01f)] private float _flashInterval = 0.1f;

    private SpriteRenderer _spriteRenderer;
    private TowerHealth _health;
    private Coroutine _flashCoroutine;

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void Initialize(TowerHealth health)
    {
        if (_health != null)
            _health.Damaged -= HandleDamaged;

        StopFlash();

        _health = health ?? throw new ArgumentNullException(nameof(health));
        _health.Damaged += HandleDamaged;
    }

    private void HandleDamaged()
    {
        StopFlash();

        if (!_health.IsAlive)
            return;

        _flashCoroutine = StartCoroutine(FlashRoutine());
    }

    private IEnumerator FlashRoutine()
    {
        WaitForSeconds wait = new(_flashInterval);

        for (int i = 0; i < _flashCount; i++)
        {
            _spriteRenderer.enabled = false;
            yield return wait;

            _spriteRenderer.enabled = true;
            yield return wait;
        }

        _flashCoroutine = null;
    }

    private void StopFlash()
    {
        if (_flashCoroutine != null)
        {
            StopCoroutine(_flashCoroutine);
            _flashCoroutine = null;
        }

        if (_spriteRenderer != null)
            _spriteRenderer.enabled = true;
    }

    private void OnDisable()
    {
        StopFlash();
    }

    private void OnDestroy()
    {
        if (_health != null)
            _health.Damaged -= HandleDamaged;
    }
}
