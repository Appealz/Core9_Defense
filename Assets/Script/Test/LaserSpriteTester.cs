using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(SpriteRenderer))]
public sealed class LaserSpriteTester : MonoBehaviour
{
    [SerializeField] private float _length = 10f;
    [SerializeField] private float _width = 0.3f;
    [SerializeField, Range(0f, 360f)] private float _angle;

    private SpriteRenderer _spriteRenderer;

    private void OnEnable()
    {
        Apply();
    }

    private void OnValidate()
    {
        Apply();
    }

    private void Apply()
    {
        if (_spriteRenderer == null)
            _spriteRenderer = GetComponent<SpriteRenderer>();

        _spriteRenderer.drawMode = SpriteDrawMode.Sliced;
        _spriteRenderer.size = new Vector2(_length, _width);

        transform.rotation = Quaternion.Euler(0f, 0f, _angle);
    }
}