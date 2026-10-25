using UnityEngine;

public sealed class RadiusAreaVisual : MonoBehaviour
{
    private SpriteRenderer _ownerRenderer;
    private SpriteRenderer _areaRenderer;

    private void Awake()
    {
        _ownerRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    public void Show(Sprite sprite, Color color, float radius)
    {
        if (sprite == null || radius <= 0f)
        {
            Hide();
            return;
        }

        EnsureRenderer();

        _areaRenderer.sprite = sprite;
        _areaRenderer.color = color;

        if (_ownerRenderer != null)
        {
            _areaRenderer.sortingLayerID = _ownerRenderer.sortingLayerID;
            _areaRenderer.sortingOrder = _ownerRenderer.sortingOrder - 1;
        }

        float diameter = radius * 2f;
        Vector2 spriteSize = sprite.bounds.size;
        Vector3 parentScale = transform.lossyScale;

        _areaRenderer.transform.localScale = new Vector3(
            diameter / (spriteSize.x * Mathf.Max(Mathf.Abs(parentScale.x), 0.0001f)),
            diameter / (spriteSize.y * Mathf.Max(Mathf.Abs(parentScale.y), 0.0001f)),
            1f);

        _areaRenderer.enabled = true;
    }

    public void Hide()
    {
        if (_areaRenderer != null)
            _areaRenderer.enabled = false;
    }

    private void EnsureRenderer()
    {
        if (_areaRenderer != null)
            return;

        GameObject areaObject = new("RadiusAreaVisual");
        areaObject.transform.SetParent(transform, false);

        _areaRenderer = areaObject.AddComponent<SpriteRenderer>();
        _areaRenderer.enabled = false;
    }

    private void OnDisable()
    {
        Hide();
    }
}
