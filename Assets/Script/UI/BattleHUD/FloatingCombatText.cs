using System.Globalization;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(RectTransform), typeof(CanvasGroup), typeof(TextMeshProUGUI))]
public sealed class FloatingCombatText : MonoBehaviour
{
    [Header("Text")]
    [SerializeField, Min(1f)] private float _fontSize = 24f;
    [SerializeField] private Color _damageColor = Color.white;
    [SerializeField] private Color _healColor = Color.green;
    [SerializeField] private Color _outlineColor = Color.black;
    [SerializeField, Range(0f, 1f)] private float _outlineWidth = 0.2f;

    [Header("Motion")]
    [SerializeField, Min(0.01f)] private float _duration = 0.7f;
    [SerializeField, Min(0f)] private float _floatDistance = 50f;
    [SerializeField, Min(0f)] private float _randomOffset = 15f;

    [Header("Fade")]
    [SerializeField, Range(0f, 1f)] private float _fadeStart = 0.4f;
    [SerializeField, Min(0.01f)] private float _fadePower = 1f;

    private RectTransform _rectTransform;
    private CanvasGroup _canvasGroup;
    private TextMeshProUGUI _text;
    private Vector2 _startPosition;
    private float _elapsed;

    public bool IsAlive => _elapsed < _duration;

    private void Awake()
    {
        CacheComponents();
    }

    public void Play(Vector2 anchoredPosition, float signedAmount)
    {
        CacheComponents();

        bool isHeal = signedAmount > 0f;
        float amount = Mathf.Abs(signedAmount);

        _startPosition = anchoredPosition + Random.insideUnitCircle * _randomOffset;
        _elapsed = 0f;

        _rectTransform.anchoredPosition = _startPosition;
        _text.fontSize = _fontSize;
        _text.color = isHeal ? _healColor : _damageColor;
        _text.outlineColor = _outlineColor;
        _text.outlineWidth = _outlineWidth;
        string formattedAmount = amount.ToString("0.##", CultureInfo.InvariantCulture);
        _text.SetText(isHeal ? "+" + formattedAmount : "-" + formattedAmount);
        _canvasGroup.alpha = 1f;

        gameObject.SetActive(true);
    }

    public void PlayMessage(Vector2 anchoredPosition, string message, Color color)
    {
        CacheComponents();

        _startPosition = anchoredPosition + Random.insideUnitCircle * _randomOffset;
        _elapsed = 0f;

        _rectTransform.anchoredPosition = _startPosition;
        _text.fontSize = _fontSize;
        _text.color = color;
        _text.outlineColor = _outlineColor;
        _text.outlineWidth = _outlineWidth;
        _text.SetText(message ?? string.Empty);
        _canvasGroup.alpha = 1f;

        gameObject.SetActive(true);
    }

    public bool Tick(float deltaTime)
    {
        _elapsed = Mathf.Min(_duration, _elapsed + Mathf.Max(0f, deltaTime));

        float normalizedTime = _duration > 0f ? _elapsed / _duration : 1f;
        _rectTransform.anchoredPosition = _startPosition + Vector2.up * (_floatDistance * normalizedTime);

        float fadeProgress = Mathf.InverseLerp(_fadeStart, 1f, normalizedTime);
        _canvasGroup.alpha = 1f - Mathf.Pow(fadeProgress, _fadePower);

        return IsAlive;
    }

    public void Clear()
    {
        CacheComponents();

        _elapsed = _duration;
        _startPosition = Vector2.zero;
        _rectTransform.anchoredPosition = Vector2.zero;
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
        _duration = Mathf.Max(0.01f, _duration);
        _floatDistance = Mathf.Max(0f, _floatDistance);
        _randomOffset = Mathf.Max(0f, _randomOffset);
        _fadePower = Mathf.Max(0.01f, _fadePower);
    }
#endif
}
