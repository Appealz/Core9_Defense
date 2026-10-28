using UnityEngine;

[DisallowMultipleComponent]
public sealed class TowerSelectionVisual : MonoBehaviour
{
    private static readonly Color SelectionColor = new(1f, 0.82f, 0.12f, 0.95f);
    private const float LineWidth = 0.07f;
    private const float Padding = 0.08f;

    private SpriteRenderer _spriteRenderer;
    private LineRenderer _lineRenderer;
    private Material _lineMaterial;

    private void Awake()
    {
        _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        CreateLineRenderer();
        SetSelected(false);
    }

    public void SetSelected(bool isSelected)
    {
        if (_lineRenderer == null)
            CreateLineRenderer();

        if (isSelected)
            RefreshBounds();

        _lineRenderer.enabled = isSelected;
    }

    public void RefreshBounds()
    {
        if (_lineRenderer == null || _spriteRenderer == null || _spriteRenderer.sprite == null)
            return;

        Bounds bounds = _spriteRenderer.sprite.bounds;
        float left = bounds.min.x - Padding;
        float right = bounds.max.x + Padding;
        float bottom = bounds.min.y - Padding;
        float top = bounds.max.y + Padding;

        _lineRenderer.SetPosition(0, new Vector3(left, bottom, -0.01f));
        _lineRenderer.SetPosition(1, new Vector3(left, top, -0.01f));
        _lineRenderer.SetPosition(2, new Vector3(right, top, -0.01f));
        _lineRenderer.SetPosition(3, new Vector3(right, bottom, -0.01f));
    }

    private void CreateLineRenderer()
    {
        if (_lineRenderer != null)
            return;

        GameObject lineObject = new("GoldUpgradeSelection");
        lineObject.transform.SetParent(transform, false);
        _lineRenderer = lineObject.AddComponent<LineRenderer>();
        _lineRenderer.useWorldSpace = false;
        _lineRenderer.loop = true;
        _lineRenderer.positionCount = 4;
        _lineRenderer.startWidth = LineWidth;
        _lineRenderer.endWidth = LineWidth;
        _lineRenderer.startColor = SelectionColor;
        _lineRenderer.endColor = SelectionColor;
        _lineRenderer.numCornerVertices = 2;
        _lineRenderer.numCapVertices = 2;

        Shader shader = Shader.Find("Sprites/Default");

        if (shader != null)
        {
            _lineMaterial = new Material(shader);
            _lineRenderer.sharedMaterial = _lineMaterial;
        }

        if (_spriteRenderer != null)
        {
            _lineRenderer.sortingLayerID = _spriteRenderer.sortingLayerID;
            _lineRenderer.sortingOrder = _spriteRenderer.sortingOrder + 10;
        }
    }

    private void OnDestroy()
    {
        if (_lineMaterial == null)
            return;

        if (Application.isPlaying)
            Destroy(_lineMaterial);
        else
            DestroyImmediate(_lineMaterial);
    }
}
