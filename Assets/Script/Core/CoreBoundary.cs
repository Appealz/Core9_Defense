using System;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class CoreBoundary : MonoBehaviour
{
    private CoreManager _coreManager;
    private BoxCollider2D _collider;

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

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.TryGetComponent(out Enemy enemy))
            return;

        enemy.ReachCore(_coreManager);
    }
}