using System;
using UnityEngine;

public sealed class CombatLayoutCoordinator : IUpdatable
{
    private const float ChangeTolerance = 0.0001f;

    private readonly CombatBoundsProvider _boundsProvider;
    private readonly CoreLayout _coreLayout;
    private CombatBounds _lastBounds;
    private Vector2 _lastPositionOffset;
    private bool _hasBounds;

    public CombatLayoutCoordinator(
        CombatBoundsProvider boundsProvider,
        CoreLayout coreLayout)
    {
        _boundsProvider = boundsProvider ?? throw new ArgumentNullException(nameof(boundsProvider));
        _coreLayout = coreLayout != null
            ? coreLayout
            : throw new ArgumentNullException(nameof(coreLayout));
    }

    public void Update(float deltaTime)
    {
        RefreshNow();
    }

    public bool RefreshNow()
    {
        CombatBounds bounds = _boundsProvider.GetBounds();
        Vector2 positionOffset = _coreLayout.PositionOffset;

        if (_hasBounds &&
            AreEquivalent(_lastBounds, bounds) &&
            Approximately(_lastPositionOffset, positionOffset))
        {
            return false;
        }

        _lastBounds = bounds;
        _lastPositionOffset = positionOffset;
        _hasBounds = true;
        WarnIfCoreExceedsCombatBounds(bounds);
        _coreLayout.SetCenter(bounds.Center);
        return true;
    }

    private void WarnIfCoreExceedsCombatBounds(CombatBounds bounds)
    {
        Vector2 center = _coreLayout.GetPositionForCombatCenter(bounds.Center);
        Vector2 halfSize = _coreLayout.GetBoundarySize() * 0.5f;

        if (center.x - halfSize.x >= bounds.Left &&
            center.x + halfSize.x <= bounds.Right &&
            center.y - halfSize.y >= bounds.Bottom &&
            center.y + halfSize.y <= bounds.Top)
        {
            return;
        }

        Debug.LogWarning(
            $"CoreLayout position offset {_coreLayout.PositionOffset} places part of the core outside CombatBounds.",
            _coreLayout);
    }

    private static bool AreEquivalent(CombatBounds first, CombatBounds second)
    {
        return Mathf.Abs(first.Left - second.Left) <= ChangeTolerance &&
               Mathf.Abs(first.Right - second.Right) <= ChangeTolerance &&
               Mathf.Abs(first.Bottom - second.Bottom) <= ChangeTolerance &&
               Mathf.Abs(first.Top - second.Top) <= ChangeTolerance;
    }

    private static bool Approximately(Vector2 first, Vector2 second)
    {
        return Mathf.Abs(first.x - second.x) <= ChangeTolerance &&
               Mathf.Abs(first.y - second.y) <= ChangeTolerance;
    }
}
