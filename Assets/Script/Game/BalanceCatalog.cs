using UnityEngine;

[CreateAssetMenu(fileName = "BalanceCatalog", menuName = "Core9/Catalogs/Balance Catalog")]
public sealed class BalanceCatalog : ScriptableObject
{
    [SerializeField] private WaveGenerationSettings _waveGeneration;
    [SerializeField] private PlayerLevelData _playerLevels;
    [SerializeField] private EconomyBalanceData _economy;
    [SerializeField] private FlyingGemSettings _flyingGems;

    public WaveGenerationSettings WaveGeneration => _waveGeneration;
    public PlayerLevelData PlayerLevels => _playerLevels;
    public EconomyBalanceData Economy => _economy;
    public FlyingGemSettings FlyingGems => _flyingGems;

    public bool TryValidate(out string error)
    {
        if (_waveGeneration == null || _playerLevels == null || _economy == null || _flyingGems == null)
        {
            error = "Balance catalog contains a missing settings asset.";
            return false;
        }

        error = null;
        return true;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (!TryValidate(out string error))
            Debug.LogError(error, this);
    }
#endif
}
