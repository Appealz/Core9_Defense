using System;

public static class WaveDifficultyCalculator
{
    public static int GetEnemyCount(int waveNumber, WaveDifficultySettings settings)
    {
        if (waveNumber < 1)
            throw new ArgumentOutOfRangeException(nameof(waveNumber));

        if (settings == null)
            throw new ArgumentNullException(nameof(settings));

        if (settings.InitialEnemyCounts.Count == 0)
            throw new WaveConfigurationException("At least one initial enemy count is required.");

        if (waveNumber <= settings.InitialEnemyCounts.Count)
        {
            int configuredCount = settings.InitialEnemyCounts[waveNumber - 1];

            if (configuredCount <= 0)
                throw new WaveConfigurationException($"Wave {waveNumber} enemy count must be positive.");

            return configuredCount;
        }

        int initialWaveCount = settings.InitialEnemyCounts.Count;
        int initialEnemyCount = settings.InitialEnemyCounts[initialWaveCount - 1];

        if (initialEnemyCount <= 0)
            throw new WaveConfigurationException($"Wave {initialWaveCount} enemy count must be positive.");

        int offset = waveNumber - initialWaveCount;
        int completedSegments = (offset - 1) / settings.WavesPerGrowthSegment;
        int positionInSegment = ((offset - 1) % settings.WavesPerGrowthSegment) + 1;
        double progress = positionInSegment / (double)settings.WavesPerGrowthSegment;
        int partialIncrease = (int)Math.Ceiling(
            settings.EnemyIncreasePerSegment * Math.Pow(progress, settings.GrowthCurveExponent));

        return initialEnemyCount +
            completedSegments * settings.EnemyIncreasePerSegment +
            partialIncrease;
    }

    public static EnemyRuntimeModifiers GetRuntimeModifiers(int waveNumber, WaveDifficultySettings settings)
    {
        if (waveNumber < 1)
            throw new ArgumentOutOfRangeException(nameof(waveNumber));

        if (settings == null)
            throw new ArgumentNullException(nameof(settings));

        float hpMultiplier = (float)Math.Pow(settings.HpMultiplierPerWave, waveNumber - 1);
        int damageStep = (waveNumber - 1) / settings.WavesPerDamageIncrease;
        float damageMultiplier = (float)Math.Pow(settings.DamageMultiplierPerStep, damageStep);
        return new EnemyRuntimeModifiers(hpMultiplier, damageMultiplier);
    }
}

public sealed class WaveConfigurationException : InvalidOperationException
{
    public WaveConfigurationException(string message) : base(message)
    {
    }
}
