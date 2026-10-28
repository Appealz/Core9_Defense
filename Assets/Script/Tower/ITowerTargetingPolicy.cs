using UnityEngine;

public interface ITowerTargetingPolicy
{
    bool TryFindTarget(
        TowerTargetSelector targetSelector,
        Vector3 origin,
        float defaultRange,
        out Enemy target);
}
