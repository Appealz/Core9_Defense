using UnityEngine;

public class TowerManager
{
    private readonly TowerFactory _towerFactory;
    private readonly CoreManager _coreManager;

    public TowerManager(TowerFactory towerFactory, CoreManager coreManager)
    {
        _towerFactory = towerFactory;
        _coreManager = coreManager;
    }

    public bool TryPlaceTower(int slotNumber, TowerPartData partData)
    {
        Tower tower = _towerFactory.Create(partData);
        return _coreManager.TryPlaceTower(slotNumber, tower);
    }
}
