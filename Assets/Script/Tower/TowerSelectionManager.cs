using System;

public sealed class TowerSelectionManager : IDisposable
{
    private readonly GameStateManager _gameStateManager;
    private readonly ITowerGoldUpgradeApplier _towerRegistry;

    public Tower SelectedTower { get; private set; }
    public event Action<Tower> SelectionChanged;

    public TowerSelectionManager(
        GameStateManager gameStateManager,
        ITowerGoldUpgradeApplier towerRegistry)
    {
        _gameStateManager = gameStateManager ?? throw new ArgumentNullException(nameof(gameStateManager));
        _towerRegistry = towerRegistry ?? throw new ArgumentNullException(nameof(towerRegistry));
        _gameStateManager.StateChanged += HandleStateChanged;
        _towerRegistry.TowerRemoved += HandleTowerRemoved;
    }

    public bool TrySelect(Tower tower)
    {
        if (_gameStateManager.CurrentState != GameState.Playing ||
            tower == null || !_towerRegistry.IsTowerActive(tower))
        {
            return false;
        }

        if (SelectedTower == tower)
            return true;

        SetSelection(tower);
        return true;
    }

    public void ClearSelection()
    {
        if (SelectedTower != null)
            SetSelection(null);
    }

    public void Dispose()
    {
        _gameStateManager.StateChanged -= HandleStateChanged;
        _towerRegistry.TowerRemoved -= HandleTowerRemoved;
        ClearSelection();
    }

    private void SetSelection(Tower tower)
    {
        Tower previous = SelectedTower;
        SelectedTower = tower;

        if (previous != null)
            previous.SetGoldUpgradeSelected(false);

        if (SelectedTower != null)
            SelectedTower.SetGoldUpgradeSelected(true);

        SelectionChanged?.Invoke(SelectedTower);
    }

    private void HandleStateChanged(GameState _, GameState nextState)
    {
        if (nextState != GameState.Playing)
            ClearSelection();
    }

    private void HandleTowerRemoved(Tower tower)
    {
        if (SelectedTower == tower)
            ClearSelection();
    }
}
