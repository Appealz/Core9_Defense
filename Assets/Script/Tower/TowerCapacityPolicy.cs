public static class TowerCapacityPolicy
{
    public static int GetMaxTowerCount(int waveNumber)
    {
        int safeWaveNumber = waveNumber < 1 ? 1 : waveNumber;

        if (safeWaveNumber <= 4)
            return 2;

        int additionalCapacity = (safeWaveNumber - 5) / 5 + 1;
        int maxTowerCount = 2 + additionalCapacity;
        return maxTowerCount > CoreBoard.SlotCount ? CoreBoard.SlotCount : maxTowerCount;
    }
}
