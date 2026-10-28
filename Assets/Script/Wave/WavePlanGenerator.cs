using System;
using System.Collections.Generic;

public sealed class WavePlanGenerator
{
    private static readonly WaveEnemyType[] EnemyTypes =
        (WaveEnemyType[])Enum.GetValues(typeof(WaveEnemyType));

    private readonly WaveGenerationSettings _settings;
    private readonly Dictionary<WaveEnemyType, WaveEnemyPhaseDefinition> _phaseByType = new();

    public WavePlanGenerator(WaveGenerationSettings settings)
    {
        _settings = settings != null ? settings : throw new ArgumentNullException(nameof(settings));
        ValidateAndIndexSettings();
    }

    public WavePlan CreatePlan(int waveNumber, int randomSeed)
    {
        if (waveNumber < 1)
            throw new ArgumentOutOfRangeException(nameof(waveNumber));

        var random = new Random(randomSeed);
        int totalCount = WaveDifficultyCalculator.GetEnemyCount(waveNumber, _settings.Difficulty);
        WavePatternDefinition pattern = SelectPattern(waveNumber, random);
        WavePatternType patternType = waveNumber <= 3
            ? WavePatternType.NormalOnly
            : pattern.PatternType;
        Dictionary<WaveEnemyType, int> counts = AllocateCounts(
            waveNumber,
            totalCount,
            pattern,
            random);
        List<WaveSpawnInstruction> schedule = CreateSchedule(counts, random);
        EnemyRuntimeModifiers modifiers = WaveDifficultyCalculator.GetRuntimeModifiers(
            waveNumber,
            _settings.Difficulty);

        return new WavePlan(waveNumber, patternType, modifiers, counts, schedule);
    }

    private WavePatternDefinition SelectPattern(int waveNumber, Random random)
    {
        if (waveNumber <= 3)
            return null;

        var candidates = new List<WavePatternDefinition>();
        double totalWeight = 0d;

        for (int i = 0; i < _settings.Patterns.Count; i++)
        {
            WavePatternDefinition pattern = _settings.Patterns[i];

            if (pattern.SelectionWeight <= 0f)
                continue;

            if (pattern.RequiresEnemy && !_phaseByType[pattern.RequiredEnemyType].IsAvailable(waveNumber))
                continue;

            candidates.Add(pattern);
            totalWeight += pattern.SelectionWeight;
        }

        if (candidates.Count == 0 || totalWeight <= 0d)
            throw new WaveConfigurationException($"Wave {waveNumber} has no eligible wave pattern.");

        double roll = random.NextDouble() * totalWeight;

        for (int i = 0; i < candidates.Count; i++)
        {
            roll -= candidates[i].SelectionWeight;

            if (roll < 0d)
                return candidates[i];
        }

        return candidates[candidates.Count - 1];
    }

    private Dictionary<WaveEnemyType, int> AllocateCounts(
        int waveNumber,
        int totalCount,
        WavePatternDefinition pattern,
        Random random)
    {
        var counts = new Dictionary<WaveEnemyType, int>();
        var maximums = new Dictionary<WaveEnemyType, int>();
        var weights = new Dictionary<WaveEnemyType, double>();
        int reservedCount = 0;

        for (int i = 0; i < EnemyTypes.Length; i++)
        {
            WaveEnemyType enemyType = EnemyTypes[i];
            WaveEnemyPhaseDefinition phase = _phaseByType[enemyType];
            bool available = phase.IsAvailable(waveNumber);
            int minimum = available ? phase.GetMinimumCount(waveNumber) : 0;
            int maximum = available
                ? (phase.MaximumCount <= 0 ? int.MaxValue : phase.MaximumCount)
                : 0;

            if (minimum > maximum)
            {
                throw new WaveConfigurationException(
                    $"Wave {waveNumber} minimum for {enemyType} exceeds its maximum.");
            }

            counts[enemyType] = minimum;
            maximums[enemyType] = maximum;
            weights[enemyType] = available
                ? (waveNumber <= 3
                    ? (enemyType == WaveEnemyType.Normal ? 1d : 0d)
                    : pattern.GetWeight(enemyType))
                : 0d;
            reservedCount += minimum;
        }

        if (reservedCount > totalCount)
        {
            throw new WaveConfigurationException(
                $"Wave {waveNumber} reserves {reservedCount} enemies but its target is {totalCount}.");
        }

        int remaining = totalCount - reservedCount;

        while (remaining > 0)
        {
            double totalWeight = 0d;

            for (int i = 0; i < EnemyTypes.Length; i++)
            {
                WaveEnemyType enemyType = EnemyTypes[i];

                if (counts[enemyType] < maximums[enemyType] && weights[enemyType] > 0d)
                    totalWeight += weights[enemyType];
            }

            if (totalWeight <= 0d)
            {
                throw new WaveConfigurationException(
                    $"Wave {waveNumber} cannot allocate its remaining {remaining} enemies. " +
                    "Check pattern weights and maximum counts.");
            }

            int allocatedByFloor = 0;
            var fractions = new Dictionary<WaveEnemyType, double>();

            for (int i = 0; i < EnemyTypes.Length; i++)
            {
                WaveEnemyType enemyType = EnemyTypes[i];

                if (counts[enemyType] >= maximums[enemyType] || weights[enemyType] <= 0d)
                    continue;

                double expected = remaining * weights[enemyType] / totalWeight;
                int capacity = maximums[enemyType] == int.MaxValue
                    ? int.MaxValue
                    : maximums[enemyType] - counts[enemyType];
                int wholeCount = Math.Min(capacity, (int)Math.Floor(expected));

                if (wholeCount > 0)
                {
                    counts[enemyType] += wholeCount;
                    allocatedByFloor += wholeCount;
                }

                if (counts[enemyType] < maximums[enemyType])
                    fractions[enemyType] = expected - Math.Floor(expected);
            }

            remaining -= allocatedByFloor;

            if (remaining <= 0)
                break;

            WaveEnemyType selectedType = SelectRemainderType(fractions, counts, maximums, weights, random);
            counts[selectedType]++;
            remaining--;
        }

        int finalCount = 0;

        for (int i = 0; i < EnemyTypes.Length; i++)
            finalCount += counts[EnemyTypes[i]];

        if (finalCount != totalCount)
        {
            throw new WaveConfigurationException(
                $"Wave {waveNumber} allocation produced {finalCount} enemies instead of {totalCount}.");
        }

        return counts;
    }

    private static WaveEnemyType SelectRemainderType(
        IReadOnlyDictionary<WaveEnemyType, double> fractions,
        IReadOnlyDictionary<WaveEnemyType, int> counts,
        IReadOnlyDictionary<WaveEnemyType, int> maximums,
        IReadOnlyDictionary<WaveEnemyType, double> weights,
        Random random)
    {
        double fractionTotal = 0d;

        foreach (KeyValuePair<WaveEnemyType, double> pair in fractions)
            fractionTotal += pair.Value;

        bool useFractions = fractionTotal > 0.0000001d;
        double total = useFractions ? fractionTotal : 0d;

        if (!useFractions)
        {
            for (int i = 0; i < EnemyTypes.Length; i++)
            {
                WaveEnemyType enemyType = EnemyTypes[i];

                if (counts[enemyType] < maximums[enemyType])
                    total += weights[enemyType];
            }
        }

        if (total <= 0d)
            throw new WaveConfigurationException("No enemy can receive the allocation remainder.");

        double roll = random.NextDouble() * total;

        for (int i = 0; i < EnemyTypes.Length; i++)
        {
            WaveEnemyType enemyType = EnemyTypes[i];

            if (counts[enemyType] >= maximums[enemyType])
                continue;

            double weight = useFractions && fractions.TryGetValue(enemyType, out double fraction)
                ? fraction
                : weights[enemyType];

            if (weight <= 0d)
                continue;

            roll -= weight;

            if (roll < 0d)
                return enemyType;
        }

        for (int i = EnemyTypes.Length - 1; i >= 0; i--)
        {
            WaveEnemyType enemyType = EnemyTypes[i];

            if (counts[enemyType] < maximums[enemyType] && weights[enemyType] > 0d)
                return enemyType;
        }

        throw new WaveConfigurationException("No enemy can receive the allocation remainder.");
    }

    private List<WaveSpawnInstruction> CreateSchedule(
        IReadOnlyDictionary<WaveEnemyType, int> counts,
        Random random)
    {
        var remainingByType = new Dictionary<WaveEnemyType, int>(counts);
        var result = new List<WaveSpawnInstruction>();
        int totalCount = 0;

        for (int i = 0; i < EnemyTypes.Length; i++)
            totalCount += remainingByType[EnemyTypes[i]];

        WaveEnemyType previousType = default;
        int consecutiveCount = 0;

        for (int spawnIndex = 0; spawnIndex < totalCount; spawnIndex++)
        {
            bool hasAlternative = false;

            for (int i = 0; i < EnemyTypes.Length; i++)
            {
                WaveEnemyType enemyType = EnemyTypes[i];

                if (remainingByType[enemyType] > 0 &&
                    (spawnIndex == 0 || enemyType != previousType))
                {
                    hasAlternative = true;
                    break;
                }
            }

            bool excludePrevious = spawnIndex > 0 &&
                consecutiveCount >= _settings.SpawnSchedule.MaximumConsecutiveSameType &&
                hasAlternative;
            int selectableCount = 0;

            for (int i = 0; i < EnemyTypes.Length; i++)
            {
                WaveEnemyType enemyType = EnemyTypes[i];

                if (remainingByType[enemyType] > 0 && (!excludePrevious || enemyType != previousType))
                    selectableCount += remainingByType[enemyType];
            }

            int roll = random.Next(selectableCount);
            WaveEnemyType selectedType = default;

            for (int i = 0; i < EnemyTypes.Length; i++)
            {
                WaveEnemyType enemyType = EnemyTypes[i];

                if (remainingByType[enemyType] <= 0 || (excludePrevious && enemyType == previousType))
                    continue;

                roll -= remainingByType[enemyType];

                if (roll < 0)
                {
                    selectedType = enemyType;
                    break;
                }
            }

            consecutiveCount = spawnIndex > 0 && selectedType == previousType
                ? consecutiveCount + 1
                : 1;
            previousType = selectedType;
            remainingByType[selectedType]--;
            float spawnTime = spawnIndex * _settings.SpawnSchedule.SpawnInterval;
            result.Add(new WaveSpawnInstruction(
                selectedType,
                _phaseByType[selectedType].EnemyConfig,
                spawnTime));
        }

        return result;
    }

    private void ValidateAndIndexSettings()
    {
        if (_settings.Difficulty == null)
            throw new WaveConfigurationException("Wave difficulty settings are missing.");

        if (_settings.SpawnSchedule == null)
            throw new WaveConfigurationException("Wave spawn schedule settings are missing.");

        for (int i = 0; i < _settings.EnemyPhases.Count; i++)
        {
            WaveEnemyPhaseDefinition phase = _settings.EnemyPhases[i];

            if (phase == null)
                throw new WaveConfigurationException($"Enemy phase {i} is null.");

            if (phase.EnemyConfig == null)
                throw new WaveConfigurationException($"Enemy config for {phase.EnemyType} is missing.");

            if (_phaseByType.ContainsKey(phase.EnemyType))
                throw new WaveConfigurationException($"Enemy phase {phase.EnemyType} is duplicated.");

            _phaseByType.Add(phase.EnemyType, phase);
        }

        for (int i = 0; i < EnemyTypes.Length; i++)
        {
            if (!_phaseByType.ContainsKey(EnemyTypes[i]))
                throw new WaveConfigurationException($"Enemy phase {EnemyTypes[i]} is missing.");
        }

        if (_phaseByType[WaveEnemyType.Normal].FirstWave != 1)
            throw new WaveConfigurationException("Normal enemy must be available from Wave 1.");

        var patternTypes = new HashSet<WavePatternType>();

        for (int i = 0; i < _settings.Patterns.Count; i++)
        {
            WavePatternDefinition pattern = _settings.Patterns[i];

            if (pattern == null)
                throw new WaveConfigurationException($"Wave pattern {i} is null.");

            if (pattern.PatternType == WavePatternType.NormalOnly)
                throw new WaveConfigurationException("NormalOnly is generated automatically and must not be configured.");

            if (!patternTypes.Add(pattern.PatternType))
                throw new WaveConfigurationException($"Wave pattern {pattern.PatternType} is duplicated.");

            if (pattern.RequiresEnemy && !_phaseByType.ContainsKey(pattern.RequiredEnemyType))
                throw new WaveConfigurationException($"Required enemy for {pattern.PatternType} is missing.");

            var weightedTypes = new HashSet<WaveEnemyType>();

            for (int weightIndex = 0; weightIndex < pattern.EnemyWeights.Count; weightIndex++)
            {
                WaveEnemyType enemyType = pattern.EnemyWeights[weightIndex].EnemyType;

                if (!weightedTypes.Add(enemyType))
                {
                    throw new WaveConfigurationException(
                        $"{pattern.PatternType} contains duplicate {enemyType} weights.");
                }
            }
        }

        if (_settings.Patterns.Count == 0)
            throw new WaveConfigurationException("At least one wave pattern is required.");

        if (_settings.Difficulty.InitialEnemyCounts.Count == 0)
            throw new WaveConfigurationException("At least one initial enemy count is required.");
    }
}
