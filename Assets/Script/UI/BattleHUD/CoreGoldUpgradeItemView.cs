using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class CoreGoldUpgradeItemView : MonoBehaviour
{
    [SerializeField] private TMP_Text _nameText;
    [SerializeField] private TMP_Text _levelText;
    [SerializeField] private TMP_Text _totalBonusText;
    [SerializeField] private TMP_Text _costText;
    [SerializeField] private Image _cardBackground;
    [SerializeField] private Button _cardButton;

    [Header("Purchase Success Animation")]
    [SerializeField, Range(0.01f, 1f)] private float _pressScale = 0.96f;
    [SerializeField, Min(0.01f)] private float _pressDuration = 0.06f;
    [SerializeField, Min(0.01f)] private float _releaseDuration = 0.10f;
    [SerializeField] private Color _highlightColor = Color.white;
    [SerializeField, Min(0.01f)] private float _highlightDuration = 0.12f;

    private static readonly Color PurchasableColor = new(0.94f, 0.95f, 0.97f, 1f);
    private static readonly Color UnavailableColor = new(0.68f, 0.70f, 0.74f, 1f);
    private static readonly Color MaxLevelColor = new(0.46f, 0.48f, 0.52f, 1f);

    private CoreGoldUpgradeData _data;
    private Action<CoreGoldUpgradeData> _purchaseRequested;
    private Action _genericPurchaseRequested;
    private string _displayName;
    private Vector3 _baseScale;
    private Color _baseColor;
    private float _successAnimationElapsed;
    private bool _isSuccessAnimating;
    private bool _isAnimationBaseInitialized;

    public CoreGoldUpgradeData Data => _data;

    private void Awake()
    {
        EnsureAnimationBase();
    }

    public void Initialize(
        CoreGoldUpgradeData data,
        Action<CoreGoldUpgradeData> purchaseRequested)
    {
        EnsureAnimationBase();
        _data = data;
        _displayName = data != null ? data.DisplayName : string.Empty;
        _purchaseRequested = purchaseRequested;
        _genericPurchaseRequested = null;
        _cardButton.onClick.RemoveListener(HandlePurchaseClicked);
        _cardButton.onClick.AddListener(HandlePurchaseClicked);
    }

    public void Initialize(string displayName, Action purchaseRequested)
    {
        EnsureAnimationBase();
        _data = null;
        _displayName = displayName ?? string.Empty;
        _purchaseRequested = null;
        _genericPurchaseRequested = purchaseRequested;
        _cardButton.onClick.RemoveListener(HandlePurchaseClicked);
        _cardButton.onClick.AddListener(HandlePurchaseClicked);
    }

    public void SetState(
        int level,
        string totalBonus,
        int nextCost,
        bool isMaxLevel,
        bool canPurchase)
    {
        _nameText.text = _displayName;
        _levelText.text = $"Lv. {level:N0}";
        _totalBonusText.text = $"Total {totalBonus}";
        _costText.text = isMaxLevel ? "MAX LEVEL" : $"Cost {nextCost:N0}";
        _baseColor = isMaxLevel
            ? MaxLevelColor
            : canPurchase ? PurchasableColor : UnavailableColor;

        if (!_isSuccessAnimating)
            _cardBackground.color = _baseColor;

        _cardButton.interactable = canPurchase;
    }

    public void PlayPurchaseSuccess()
    {
        EnsureAnimationBase();
        ResetSuccessVisuals();
        _successAnimationElapsed = 0f;
        _isSuccessAnimating = true;
    }

    private void HandlePurchaseClicked()
    {
        if (!_cardButton.interactable)
            return;

        if (_data != null)
            _purchaseRequested?.Invoke(_data);
        else
            _genericPurchaseRequested?.Invoke();
    }

    private void Update()
    {
        UpdateSuccessAnimation(Time.unscaledDeltaTime);
    }

    private void OnDisable()
    {
        ResetSuccessAnimation();
    }

    private void UpdateSuccessAnimation(float deltaTime)
    {
        if (!_isSuccessAnimating)
            return;

        float totalScaleDuration = _pressDuration + _releaseDuration;
        _successAnimationElapsed = Mathf.Min(
            Mathf.Max(totalScaleDuration, _highlightDuration),
            _successAnimationElapsed + Mathf.Max(0f, deltaTime));

        float scaleMultiplier;

        if (_successAnimationElapsed <= _pressDuration)
        {
            float pressTime = _pressDuration > 0f
                ? _successAnimationElapsed / _pressDuration
                : 1f;
            scaleMultiplier = Mathf.Lerp(1f, _pressScale, pressTime);
        }
        else
        {
            float releaseTime = _releaseDuration > 0f
                ? (_successAnimationElapsed - _pressDuration) / _releaseDuration
                : 1f;
            scaleMultiplier = Mathf.Lerp(_pressScale, 1f, Mathf.Clamp01(releaseTime));
        }

        transform.localScale = _baseScale * scaleMultiplier;

        float highlightTime = _highlightDuration > 0f
            ? Mathf.Clamp01(_successAnimationElapsed / _highlightDuration)
            : 1f;
        float highlightStrength = Mathf.Sin(Mathf.PI * highlightTime);
        _cardBackground.color = Color.Lerp(_baseColor, _highlightColor, highlightStrength);

        if (_successAnimationElapsed < Mathf.Max(totalScaleDuration, _highlightDuration))
            return;

        ResetSuccessAnimation();
    }

    private void ResetSuccessAnimation()
    {
        _isSuccessAnimating = false;
        _successAnimationElapsed = 0f;
        ResetSuccessVisuals();
    }

    private void ResetSuccessVisuals()
    {
        if (!_isAnimationBaseInitialized)
            return;

        transform.localScale = _baseScale;

        if (_cardBackground != null)
            _cardBackground.color = _baseColor;
    }

    private void EnsureAnimationBase()
    {
        if (_isAnimationBaseInitialized)
            return;

        _baseScale = transform.localScale;

        if (_cardBackground != null)
            _baseColor = _cardBackground.color;

        _isAnimationBaseInitialized = true;
    }

    private void OnDestroy()
    {
        ResetSuccessAnimation();

        if (_cardButton != null)
            _cardButton.onClick.RemoveListener(HandlePurchaseClicked);
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        _pressScale = Mathf.Clamp(_pressScale, 0.01f, 1f);
        _pressDuration = Mathf.Max(0.01f, _pressDuration);
        _releaseDuration = Mathf.Max(0.01f, _releaseDuration);
        _highlightDuration = Mathf.Max(0.01f, _highlightDuration);
    }
#endif
}
