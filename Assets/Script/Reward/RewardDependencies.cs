using UnityEngine;

public class RewardDependencies
{
    public TowerManager TowerManager { get; }
    public GoldManager GoldManager { get; }
    public CoreSlotEnhancementSelectionController CoreSlotEnhancementSelection { get; }

    public RewardDependencies(
        TowerManager towerManager,
        GoldManager goldManager,
        CoreSlotEnhancementSelectionController coreSlotEnhancementSelection)
    {
        TowerManager = towerManager;
        GoldManager = goldManager;
        CoreSlotEnhancementSelection = coreSlotEnhancementSelection;
    }
}
