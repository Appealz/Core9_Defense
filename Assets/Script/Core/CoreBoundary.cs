using System;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class CoreBoundary : MonoBehaviour
{
    private CoreManager _coreManager;
    private BoxCollider2D _collider;

    public bool CanReceiveDamage => _coreManager != null && _coreManager.HasAnyTower();

    private void Awake()
    {
        CoreLayout coreLayout = GetComponentInParent<CoreLayout>();

        _collider = GetComponent<BoxCollider2D>();
        _collider.isTrigger = true;
        _collider.size = coreLayout.GetBoundarySize();
    }

    public void Initialize(CoreManager coreManager)
    {
        _coreManager = coreManager ?? throw new ArgumentNullException(nameof(coreManager));
    }

    public Vector3 GetClosestPoint(Vector3 worldPosition)
    {
        Vector2 point = _collider.ClosestPoint(worldPosition);
        return new Vector3(point.x, point.y, worldPosition.z);
    }

    public bool TryTakeDamage(float damage)
    {
        return _coreManager != null && _coreManager.TryApplyDamage(damage);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.TryGetComponent(out Enemy enemy))
            return;

        enemy.ReachCore();
    }
}
