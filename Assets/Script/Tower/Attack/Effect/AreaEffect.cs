using UnityEngine;

public sealed class AreaEffect : ITowerAttackEffect
{
    private readonly Sprite _sprite;
    private readonly float _alpha;

    public AreaEffect(Sprite sprite, float alpha)
    {
        _sprite = sprite;
        _alpha = Mathf.Clamp01(alpha);
    }

    public void Show(Transform owner, TowerStats stats)
    {
        if (owner == null || _sprite == null)
            return;

        GameObject effectObject = new GameObject("AreaEffect");
        effectObject.transform.SetParent(owner, false);

        SpriteRenderer spriteRenderer = effectObject.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = _sprite;
        spriteRenderer.color = new Color(1f, 1f, 1f, _alpha);

        SpriteRenderer towerRenderer = owner.GetComponentInChildren<SpriteRenderer>();

        if (towerRenderer != null)
        {
            spriteRenderer.sortingLayerID = towerRenderer.sortingLayerID;
            spriteRenderer.sortingOrder = towerRenderer.sortingOrder - 1;
        }

        float diameter = stats.AttackRange * 2f;
        Vector2 spriteSize = _sprite.bounds.size;
        Vector3 parentScale = owner.lossyScale;

        effectObject.transform.localScale = new Vector3(
            diameter / (spriteSize.x * Mathf.Abs(parentScale.x)),
            diameter / (spriteSize.y * Mathf.Abs(parentScale.y)),
            1f);
    }
}