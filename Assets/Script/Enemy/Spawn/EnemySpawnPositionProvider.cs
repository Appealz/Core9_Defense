using UnityEngine;

public sealed class EnemySpawnPositionProvider
{
    private readonly CoreLayout _coreLayout;
    private readonly CombatBoundsProvider _boundsProvider;
    private readonly SpawnPositionCalculator _calculator;

    private readonly float _spawnMargin;

    public EnemySpawnPositionProvider(
        CoreLayout coreLayout,
        CombatBoundsProvider boundsProvider,
        SpawnPositionCalculator calculator,
        float spawnMargin)
    {
        _coreLayout = coreLayout;
        _boundsProvider = boundsProvider;
        _calculator = calculator;
        _spawnMargin = spawnMargin;
    }

    public Vector2 GetPosition()
    {
        Vector2 origin =
            _coreLayout.transform.position;

        CombatBounds bounds =
            _boundsProvider.GetBounds();

        float angle =
            Random.Range(0f, Mathf.PI * 2f);

        Vector2 direction = new(
            Mathf.Cos(angle),
            Mathf.Sin(angle));

        return _calculator.Calculate(
            bounds,
            origin,
            direction,
            _spawnMargin);
    }
}
