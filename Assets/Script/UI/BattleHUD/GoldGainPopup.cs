
using TMPro;
using UnityEngine;

[RequireComponent(
    typeof(RectTransform),
    typeof(CanvasGroup),
    typeof(TextMeshProUGUI))]
public sealed class GoldGainPopup : MonoBehaviour
{
    [Header("Gold Gain")]
    [SerializeField, Min(1f)]
    private float _fontSize = 30f;

    [SerializeField]
    private Color _gainColor = new(1f, 0.78f, 0.15f, 1f);

    [SerializeField, Min(0.01f)]
    private float _displayDuration = 0.35f;

    [SerializeField, Min(0f)]
    private float _floatDistance = 35f;

    [SerializeField, Min(0f)]
    private float _randomOffset = 8f;

    [Header("Spawn Position")]
    [SerializeField]
    private Vector2 _spawnOffset = new(0f, 25f);

    [Header("Gold Spend")]
    [SerializeField]
    private Color _spendColor = new(1f, 0.25f, 0.25f, 1f);

    [SerializeField, Min(1f)]
    private float _spendFontSize = 30f;

    [SerializeField]
    private Vector2 _spendSpawnOffset = new(0f, -25f);

    [SerializeField, Min(0.01f)]
    private float _spendDisplayDuration = 0.35f;

    [SerializeField, Min(0f)]
    private float _spendFloatDistance = 35f;

    [SerializeField, Min(0f)]
    private float _spendRandomOffset = 8f;

    private RectTransform _rectTransform;
    private CanvasGroup _canvasGroup;
    private TextMeshProUGUI _text;
    private Vector2 _startPosition;
    private float _elapsed;
    private float _activeDuration;
    private float _activeFloatDistance;

    public bool IsAlive => _elapsed < _activeDuration;

    private void Awake()
    {
        CacheComponents();
    }

    public void Play(Vector2 anchoredPosition, int amount)
    {
        Play(
            anchoredPosition,
            amount,
            "+",
            _fontSize,
            _gainColor,
            _spawnOffset,
            _displayDuration,
            _floatDistance,
            _randomOffset);
    }

    public void PlaySpend(Vector2 anchoredPosition, int amount)
    {
        Play(
            anchoredPosition,
            amount,
            "-",
            _spendFontSize,
            _spendColor,
            _spendSpawnOffset,
            _spendDisplayDuration,
            -_spendFloatDistance,
            _spendRandomOffset);
    }

    private void Play(
        Vector2 anchoredPosition,
        int amount,
        string prefix,
        float fontSize,
        Color color,
        Vector2 spawnOffset,
        float displayDuration,
        float floatDistance,
        float randomOffset)
    {
        CacheComponents();

        _startPosition = anchoredPosition
            + spawnOffset
            + Random.insideUnitCircle * randomOffset;

        _elapsed = 0f;
        _activeDuration = displayDuration;
        _activeFloatDistance = floatDistance;

        _rectTransform.anchoredPosition = _startPosition;
        _rectTransform.localScale = Vector3.one;
        _text.fontSize = fontSize;
        _text.color = color;
        _text.text = prefix + amount.ToString("N0");

        _canvasGroup.alpha = 1f;
        gameObject.SetActive(true);
    }

    public bool Tick(float deltaTime)
    {
        _elapsed = Mathf.Min(
            _activeDuration,
            _elapsed + Mathf.Max(0f, deltaTime));

        float normalizedTime = _activeDuration > 0f
            ? _elapsed / _activeDuration
            : 1f;

        _rectTransform.anchoredPosition =
            _startPosition
            + Vector2.up * (_activeFloatDistance * normalizedTime);

        _canvasGroup.alpha = 1f - normalizedTime;

        return IsAlive;
    }

    public void Clear()
    {
        CacheComponents();

        _elapsed = 0f;
        _activeDuration = 0f;
        _activeFloatDistance = 0f;
        _startPosition = Vector2.zero;

        _rectTransform.anchoredPosition = Vector2.zero;
        _rectTransform.localScale = Vector3.one;

        _canvasGroup.alpha = 1f;
        _text.SetText(string.Empty);
    }

    private void CacheComponents()
    {
        if (_rectTransform == null)
            _rectTransform = (RectTransform)transform;

        if (_canvasGroup == null)
            _canvasGroup = GetComponent<CanvasGroup>();

        if (_text == null)
            _text = GetComponent<TextMeshProUGUI>();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        _fontSize = Mathf.Max(1f, _fontSize);
        _displayDuration = Mathf.Max(0.01f, _displayDuration);
        _floatDistance = Mathf.Max(0f, _floatDistance);
        _randomOffset = Mathf.Max(0f, _randomOffset);
        _spendFontSize = Mathf.Max(1f, _spendFontSize);
        _spendDisplayDuration = Mathf.Max(0.01f, _spendDisplayDuration);
        _spendFloatDistance = Mathf.Max(0f, _spendFloatDistance);
        _spendRandomOffset = Mathf.Max(0f, _spendRandomOffset);
    }
#endif
}
