using System;
using UnityEngine;

[Serializable]
public sealed class FlyingGemSettings
{
    [Header("Spawn")]
    [SerializeField, Min(0.1f)] private float _spawnIntervalMin = 5f;
    [SerializeField, Min(0.1f)] private float _spawnIntervalMax = 10f;
    [SerializeField, Min(1)] private int _maxActiveGems = 1;

    [Header("Movement")]
    [SerializeField, Min(0f)] private float _moveSpeed = 3f;
    [SerializeField, Range(0f, 45f)] private float _bounceRandomAngle = 8f;
    [SerializeField, Min(0f)] private float _leftMargin = 1f;
    [SerializeField, Min(0f)] private float _rightMargin = 1f;
    [SerializeField, Min(0f)] private float _bottomMargin = 2.5f;
    [SerializeField, Min(0f)] private float _topMargin = 7f;

    [Header("Appearance")]
    [SerializeField, Min(0.1f)] private float _visualScale = 1f;
    [SerializeField, Min(1f)] private float _touchHitboxScale = 1.5f;

    [Header("Lifetime")]
    [SerializeField, Min(0.1f)] private float _lifetimeMin = 60f;
    [SerializeField, Min(0.1f)] private float _lifetimeMax = 120f;

    [Header("Reward")]
    [SerializeField, Min(1)] private int _rewardMin = 1;
    [SerializeField, Min(1)] private int _rewardMax = 3;

    [Header("Touch Hint")]
    [SerializeField] private bool _enableTouchHint = true;
    [SerializeField, Min(0.1f)] private float _touchHintInterval = 30f;
    [SerializeField, Min(0.1f)] private float _touchHintDuration = 2f;
    [SerializeField, Min(1)] private int _touchHintBlinkCount = 3;

    public float SpawnIntervalMin => _spawnIntervalMin;
    public float SpawnIntervalMax => _spawnIntervalMax;
    public int MaxActiveGems => _maxActiveGems;
    public float MoveSpeed => _moveSpeed;
    public float BounceRandomAngle => _bounceRandomAngle;
    public float LeftMargin => _leftMargin;
    public float RightMargin => _rightMargin;
    public float BottomMargin => _bottomMargin;
    public float TopMargin => _topMargin;
    public float VisualScale => _visualScale;
    public float TouchHitboxScale => _touchHitboxScale;
    public float LifetimeMin => _lifetimeMin;
    public float LifetimeMax => _lifetimeMax;
    public int RewardMin => _rewardMin;
    public int RewardMax => _rewardMax;
    public bool EnableTouchHint => _enableTouchHint;
    public float TouchHintInterval => _touchHintInterval;
    public float TouchHintDuration => _touchHintDuration;
    public int TouchHintBlinkCount => _touchHintBlinkCount;

    public void Validate()
    {
        _spawnIntervalMin = Mathf.Max(0.1f, _spawnIntervalMin);
        _spawnIntervalMax = Mathf.Max(_spawnIntervalMin, _spawnIntervalMax);
        _maxActiveGems = 1;
        _moveSpeed = Mathf.Max(0f, _moveSpeed);
        _bounceRandomAngle = Mathf.Clamp(_bounceRandomAngle, 0f, 45f);
        _leftMargin = Mathf.Max(0f, _leftMargin);
        _rightMargin = Mathf.Max(0f, _rightMargin);
        _bottomMargin = Mathf.Max(0f, _bottomMargin);
        _topMargin = Mathf.Max(0f, _topMargin);
        _visualScale = Mathf.Max(0.1f, _visualScale);
        _touchHitboxScale = Mathf.Max(1f, _touchHitboxScale);
        _lifetimeMin = Mathf.Max(0.1f, _lifetimeMin);
        _lifetimeMax = Mathf.Max(_lifetimeMin, _lifetimeMax);
        _rewardMin = Mathf.Max(1, _rewardMin);
        _rewardMax = Mathf.Max(_rewardMin, _rewardMax);
        _touchHintInterval = Mathf.Max(0.1f, _touchHintInterval);
        _touchHintDuration = Mathf.Max(0.1f, _touchHintDuration);
        _touchHintBlinkCount = Mathf.Max(1, _touchHintBlinkCount);
    }
}
