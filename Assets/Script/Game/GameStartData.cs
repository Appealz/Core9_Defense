using UnityEngine;

[CreateAssetMenu(fileName = "GameStartData", menuName = "Scriptable Objects/GameStartData")]
public class GameStartData : ScriptableObject
{
    [SerializeField] private EnemyCatalog _enemies;
    [SerializeField] private TowerCatalog _towers;
    [SerializeField] private RewardCatalog _rewards;
    [SerializeField] private BalanceCatalog _balance;

    public EnemyCatalog Enemies => _enemies;
    public TowerCatalog Towers => _towers;
    public RewardCatalog Rewards => _rewards;
    public BalanceCatalog Balance => _balance;

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (_enemies == null || _towers == null || _rewards == null || _balance == null)
            Debug.LogError("GameStartData requires all four domain catalogs.", this);
    }
#endif
}
