using UnityEngine;

[CreateAssetMenu(fileName = "GameStartData", menuName = "Scriptable Objects/GameStartData")]
public class GameStartData : ScriptableObject
{
    [SerializeField] private int _initialTowerSlotNumber = 5;
    [SerializeField] private TowerPartData _basicTowerPartData;

    public int InitialTowerSlotNumber => _initialTowerSlotNumber;
    public TowerPartData BasicTowerPartData => _basicTowerPartData;
}
