
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
        tower.Health.Died += () => HandleTowerDied(tower);

        return true;
    }

    public bool CanPlaceNextTower()
    {
        return _coreManager.TryFindPlacementSlotNumber(out _);
    }

    public async UniTask<bool> TryPlaceNextTowerAsync(TowerPartData partData)
    {
        if (!_coreManager.TryFindPlacementSlotNumber(out int slotNumber))
            return false;

        return await TryPlaceTowerAsync(slotNumber, partData);
    }

    public bool TryMoveOrSwapTower(int fromSlotNumber, int toSlotNumber)
    {
        if (!_coreManager.TryGetTower(fromSlotNumber, out Tower movingTower))
            return false;

        _coreManager.TryGetTower(toSlotNumber, out Tower targetTower);

        if (!_coreManager.TryMoveOrSwapTower(fromSlotNumber, toSlotNumber))
            return false;

        movingTower.transform.position = _coreLayout.GetSlotPosition(toSlotNumber);

        if (targetTower != null)
            targetTower.transform.position = _coreLayout.GetSlotPosition(fromSlotNumber);

        return true;
    }

    private void HandleTowerDied(Tower tower)
    {
        if (!_coreManager.TryRemoveTower(tower))
            return;

        _towerFactory.Release(tower);

        if (!_coreManager.HasAnyTower())
            AllTowersDestroyed?.Invoke();
    }
}
