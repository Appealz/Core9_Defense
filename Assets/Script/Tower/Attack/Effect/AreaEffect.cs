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

        RadiusAreaVisual areaVisual = owner.GetComponent<RadiusAreaVisual>();

        if (areaVisual == null)
            areaVisual = owner.gameObject.AddComponent<RadiusAreaVisual>();

        areaVisual.Show(_sprite, new Color(1f, 1f, 1f, _alpha), stats.AttackRange);
    }
}
