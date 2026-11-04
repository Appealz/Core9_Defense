
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GoldHUDView : MonoBehaviour
{
    private const int InitialPoolSize = 8;

    [SerializeField] private TMP_Text _goldAmountText;
    [SerializeField] private RectTransform _gainPopupRoot;
    [SerializeField] private GoldGainPopup _gainPopupPrefab;

    [Header("Punch")]
    [SerializeField, Min(1f)] private float _punchScale = 1.15f;
    [SerializeField, Min(0.01f)] private float _punchDuration = 0.18f;

    [Header("Gold Count Animation")]
    [SerializeField, Min(0.01f)] private float _countDuration = 0.3f;
    [SerializeField, Min(1f)] private float _countEaseOutPower = 3f;

    private readonly Queue<GoldGainPopup> _popupPool = new();
    private readonly List<GoldGainPopup> _activePopups = new(InitialPoolSize);

    private RectTransform _goldAmountRect;
    private Vector3 _goldAmountBaseScale;
    private float _punchElapsed;
    private bool _isPunching;
    private int _displayedGold;
    private int _countStartGold;
    private int _targetGold;
    private float _countElapsed;
    private bool _isCounting;
    private bool _hasGoldValue;
    private bool _isInitialized;

    public bool IsConfigured =>
        _goldAmountText != null && _gainPopupRoot != null && _gainPopupPrefab != null;

    private void Awake()
    {
        EnsureInitialized();
    }

    public void SetGold(int amount)
    {
        int nextGold = Mathf.Max(0, amount);

        if (!_hasGoldValue)
        {
            _hasGoldValue = true;
            _targetGold = nextGold;
            ApplyDisplayedGold(nextGold);
            return;
        }

        if (_isCounting && _targetGold == nextGold)
            return;

        _countStartGold = _displayedGold;
        _targetGold = nextGold;
        _countElapsed = 0f;
        _isCounting = _countStartGold != _targetGold;

        if (!_isCounting)
            ApplyDisplayedGold(_targetGold);
    }

    public void PlayGoldGain(int amount)
    {
        if (!TryPlayPopup(amount, isSpend: false))
            return;

        _goldAmountRect.localScale = _goldAmountBaseScale;
        _punchElapsed = 0f;
        _isPunching = true;
    }

    public void PlayGoldSpend(int amount)
    {
        TryPlayPopup(amount, isSpend: true);
    }

    private void Update()
    {
        float deltaTime = Time.unscaledDeltaTime;

        for (int i = _activePopups.Count - 1; i >= 0; i--)
        {
            GoldGainPopup popup = _activePopups[i];

            if (popup.Tick(deltaTime))
                continue;

            ReturnPopup(i, popup);
        }

        UpdateGoldCount(deltaTime);
        UpdatePunch(deltaTime);
    }

    private bool TryPlayPopup(int amount, bool isSpend)
    {
        if (amount <= 0)
            return false;

        EnsureInitialized();

        if (!_isInitialized)
            return false;

        GoldGainPopup popup = GetPopup();

        if (isSpend)
            popup.PlaySpend(_goldAmountRect.anchoredPosition, amount);
        else
            popup.Play(_goldAmountRect.anchoredPosition, amount);

        _activePopups.Add(popup);
        return true;
    }

    private void OnDisable()
    {
        for (int i = _activePopups.Count - 1; i >= 0; i--)
            ReturnPopup(i, _activePopups[i]);

        if (_goldAmountRect != null)
            _goldAmountRect.localScale = _goldAmountBaseScale;

        _punchElapsed = 0f;
        _isPunching = false;
        CompleteGoldCount();
    }

    private void EnsureInitialized()
    {
        if (_isInitialized || !IsConfigured)
            return;

        _goldAmountRect = _goldAmountText.rectTransform;
        _goldAmountBaseScale = _goldAmountRect.localScale;

        for (int i = 0; i < InitialPoolSize; i++)
            _popupPool.Enqueue(CreatePopup());

        _isInitialized = true;
    }

    private GoldGainPopup GetPopup()
    {
        return _popupPool.Count > 0
            ? _popupPool.Dequeue()
            : CreatePopup();
    }

    private GoldGainPopup CreatePopup()
    {
        GoldGainPopup popup = Instantiate(_gainPopupPrefab, _gainPopupRoot, false);
        popup.gameObject.SetActive(false);
        return popup;
    }

    private void ReturnPopup(int index, GoldGainPopup popup)
    {
        _activePopups.RemoveAt(index);
        popup.Clear();
        popup.gameObject.SetActive(false);
        _popupPool.Enqueue(popup);
    }

    private void UpdatePunch(float deltaTime)
    {
        if (!_isPunching)
            return;

        _punchElapsed = Mathf.Min(_punchDuration, _punchElapsed + Mathf.Max(0f, deltaTime));
        float normalizedTime = _punchDuration > 0f ? _punchElapsed / _punchDuration : 1f;
        float scaleMultiplier = 1f + (_punchScale - 1f) * Mathf.Sin(Mathf.PI * normalizedTime);
        _goldAmountRect.localScale = _goldAmountBaseScale * scaleMultiplier;

        if (_punchElapsed < _punchDuration)
            return;

        _goldAmountRect.localScale = _goldAmountBaseScale;
        _isPunching = false;
    }

    private void UpdateGoldCount(float deltaTime)
    {
        if (!_isCounting)
            return;

        _countElapsed = Mathf.Min(
            _countDuration,
            _countElapsed + Mathf.Max(0f, deltaTime));

        float normalizedTime = _countDuration > 0f
            ? _countElapsed / _countDuration
            : 1f;
        float easedTime = 1f - Mathf.Pow(1f - normalizedTime, _countEaseOutPower);
        double interpolated = _countStartGold +
            ((long)_targetGold - _countStartGold) * easedTime;
        long rounded = (long)System.Math.Round(
            interpolated,
            System.MidpointRounding.AwayFromZero);
        int displayed = rounded <= 0L
            ? 0
            : rounded >= int.MaxValue ? int.MaxValue : (int)rounded;

        ApplyDisplayedGold(displayed);

        if (_countElapsed < _countDuration)
            return;

        CompleteGoldCount();
    }

    private void CompleteGoldCount()
    {
        if (!_hasGoldValue)
            return;

        _isCounting = false;
        _countElapsed = 0f;
        _countStartGold = _targetGold;
        ApplyDisplayedGold(_targetGold);
    }

    private void ApplyDisplayedGold(int amount)
    {
        _displayedGold = Mathf.Max(0, amount);
        _goldAmountText.text = _displayedGold.ToString("N0");
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        _punchScale = Mathf.Max(1f, _punchScale);
        _punchDuration = Mathf.Max(0.01f, _punchDuration);
        _countDuration = Mathf.Max(0.01f, _countDuration);
        _countEaseOutPower = Mathf.Max(1f, _countEaseOutPower);
    }
#endif
}
