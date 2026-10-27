
using System;
using DG.Tweening;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class EnemyVisual : MonoBehaviour
{
    private SpriteRenderer _spriteRenderer;
    private Tween _spawnTween;
    private Tween _retreatTween;
    private Vector3 _defaultScale;
    private Color _defaultColor;

    public bool IsSpawnSpreading { get; private set; }

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _defaultScale = transform.localScale;
        _defaultColor = _spriteRenderer.color;
    }

    public void Initialize(Sprite sprite)
    {
        StopSpawnSpread();
        StopRetreat();

        _spriteRenderer.sprite = sprite;
        _spriteRenderer.color = _defaultColor;
        transform.localScale = _defaultScale;
    }

    public void PlaySpawnSpread(Vector3 targetPosition, float duration)
    {
        StopSpawnSpread();

        if (duration <= 0f)
        {
            transform.position = targetPosition;
            return;
        }

        IsSpawnSpreading = true;

        _spawnTween = transform.DOMove(targetPosition, duration)
            .SetEase(Ease.OutCubic)
            .OnComplete(() =>
            {
                IsSpawnSpreading = false;
                _spawnTween = null;
            });
    }

    public void StopSpawnSpread()
    {
        _spawnTween?.Kill();
        _spawnTween = null;
        IsSpawnSpreading = false;
    }

    public void PlayRetreat(float duration, Action completed)
    {
        StopSpawnSpread();
        StopRetreat();

        float safeDuration = Mathf.Max(0.01f, duration);
        Sequence sequence = DOTween.Sequence();
        sequence.Join(_spriteRenderer.DOFade(0f, safeDuration));
        sequence.Join(transform.DOScale(_defaultScale * 0.75f, safeDuration));
        sequence.SetEase(Ease.InCubic);
        sequence.OnComplete(() =>
        {
            _retreatTween = null;
            completed?.Invoke();
        });
        _retreatTween = sequence;
    }

    private void StopRetreat()
    {
        _retreatTween?.Kill();
        _retreatTween = null;
    }

    private void OnDisable()
    {
        StopSpawnSpread();
        StopRetreat();
    }

    private void OnDestroy()
    {
        StopSpawnSpread();
        StopRetreat();
    }
}
