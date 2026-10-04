using System;
using Cysharp.Threading.Tasks;

public class TowerManager
{
    public event Action AllTowersDestroyed;

    private readonly TowerFactory _towerFactory;
    private readonly CoreManager _coreManager;
    private readonly CoreLayout _coreLayout;

    public TowerManager(TowerFactory towerFactory, CoreManager coreManager, CoreLayout coreLayout)
    {
        _towerFactory = towerFactory;
        _coreManager = coreManager;
        _coreLayout = coreLayout;
    }

    public async UniTask<bool> TryPlaceTowerAsync(int slotNumber, TowerPartData partData)
    {
        Tower tower = await _towerFactory.CreateAsync(partData);

        if (!_coreManager.TryPlaceTower(slotNumber, tower))
        {
            _towerFactory.Release(tower);
            return false;
        }

        tower.transform.position = _coreLayout.GetSlotPosition(slotNumber);
        tower.Health.Died += () => HandleTowerDied(slotNumber, tower);

        return true;
    }

    public async UniTask<bool> TryPlaceNextTowerAsync(TowerPartData partData)
    {
        if (!_coreManager.TryFindPlacementSlotNumber(out int slotNumber))
            return false;

        return await TryPlaceTowerAsync(slotNumber, partData);
    }

    private void HandleTowerDied(int slotNumber, Tower tower)
    {
        if (!_coreManager.TryRemoveTower(slotNumber, out Tower removedTower))
            return;

        if (removedTower != tower)
            return;

        _towerFactory.Release(tower);

        if (!_coreManager.HasAnyTower())
            AllTowersDestroyed?.Invoke();
    }
}