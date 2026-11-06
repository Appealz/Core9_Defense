using UnityEngine;

[CreateAssetMenu(fileName = "EconomyBalanceData", menuName = "Core9/Balance/Economy Balance")]
public sealed class EconomyBalanceData : ScriptableObject
{
    [SerializeField, Min(0)] private int _scorePerEnemy = 100;
    [SerializeField, Min(0)] private int _baseCoins = 30;
    [SerializeField, Min(0)] private int _coinsPerWave = 10;
    [SerializeField, Range(0f, 1f)] private float _earlyExitCoinsPenaltyRate = 0.2f;

    public int ScorePerEnemy => Mathf.Max(0, _scorePerEnemy);
    public int BaseCoins => Mathf.Max(0, _baseCoins);
    public int CoinsPerWave => Mathf.Max(0, _coinsPerWave);
    public float EarlyExitCoinsPenaltyRate => Mathf.Clamp01(_earlyExitCoinsPenaltyRate);
}
