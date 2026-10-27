
using DG.Tweening;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class EnemyVisual : MonoBehaviour
{
    private SpriteRenderer _spriteRenderer;
    private Tween _spawnTween;
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

    private void OnDisable()
    {
        StopSpawnSpread();
    }

    private void OnDestroy()
    {
        StopSpawnSpread();
    }
}
