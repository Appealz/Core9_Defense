
using UnityEngine;

public class CoreRearrangementVisual : MonoBehaviour
{
    [SerializeField, Range(0f, 1f)] private float _dimAlpha = 0.65f;
    [SerializeField, Min(1f)] private float _selectedScale = 1.15f;
    [SerializeField] private float _liftHeight = 0.4f;

    private const int DimmerSortingOrder = 32000;
    private const int HighlightSortingOrder = 32001;

    private Camera _mainCamera;
    private SpriteRenderer _dimmer;
    private SpriteRenderer _highlight;
    private SpriteRenderer _selectedRenderer;
    private Sprite _dimmerSprite;

    public void Initialize(Camera mainCamera)
    {
        _mainCamera = mainCamera;

        if (_mainCamera == null || !_mainCamera.orthographic)
        {
            Debug.LogError("재배치 연출에는 Orthographic Camera가 필요합니다.", this);
            return;
        }

        CreateDimmer();
        CreateHighlight();
    }

    public void ShowSelection(Tower tower)
    {
        if (tower == null || _dimmer == null || _highlight == null)
            return;

        _selectedRenderer = tower.GetComponentInChildren<SpriteRenderer>();

        if (_selectedRenderer == null)
            return;

        _highlight.sprite = _selectedRenderer.sprite;
        _highlight.color = _selectedRenderer.color;
        _highlight.flipX = _selectedRenderer.flipX;
        _highlight.flipY = _selectedRenderer.flipY;
        _highlight.drawMode = _selectedRenderer.drawMode;
        _highlight.size = _selectedRenderer.size;

        _highlight.transform.rotation = _selectedRenderer.transform.rotation;
        _highlight.transform.localScale = _selectedRenderer.transform.lossyScale * _selectedScale;

        _dimmer.enabled = true;
        _highlight.enabled = true;

        UpdateDimmer();
    }

    public void SetDragPosition(Vector3 worldPosition)
    {
        if (_selectedRenderer == null || !_highlight.enabled)
            return;

        _highlight.transform.position = new Vector3(
            worldPosition.x,
            worldPosition.y + _liftHeight,
            _selectedRenderer.transform.position.z
        );
    }

    public void HideSelection()
    {
        _selectedRenderer = null;

        if (_dimmer != null)
            _dimmer.enabled = false;

        if (_highlight != null)
            _highlight.enabled = false;
    }

    private void CreateDimmer()
    {
        Texture2D texture = Texture2D.whiteTexture;

        _dimmerSprite = Sprite.Create(
            texture,
            new Rect(0, 0, texture.width, texture.height),
            new Vector2(0.5f, 0.5f),
            texture.width
        );

        GameObject dimmerObject = new("RearrangementDimmer");

        _dimmer = dimmerObject.AddComponent<SpriteRenderer>();
        _dimmer.sprite = _dimmerSprite;
        _dimmer.color = new Color(0f, 0f, 0f, _dimAlpha);
        _dimmer.sortingOrder = DimmerSortingOrder;
        _dimmer.enabled = false;
    }

    private void CreateHighlight()
    {
        GameObject highlightObject = new("RearrangementHighlight");

        _highlight = highlightObject.AddComponent<SpriteRenderer>();
        _highlight.sortingOrder = HighlightSortingOrder;
        _highlight.enabled = false;
    }

    private void LateUpdate()
    {
        if (_dimmer == null || !_dimmer.enabled)
            return;

        UpdateDimmer();
    }

    private void UpdateDimmer()
    {
        float height = _mainCamera.orthographicSize * 2f;
        float width = height * _mainCamera.aspect;

        _dimmer.transform.position = _mainCamera.transform.position
            + _mainCamera.transform.forward * (_mainCamera.nearClipPlane + 1f);

        _dimmer.transform.rotation = _mainCamera.transform.rotation;
        _dimmer.transform.localScale = new Vector3(
            width / _dimmerSprite.bounds.size.x,
            height / _dimmerSprite.bounds.size.y,
            1f
        );
    }

    private void OnDisable()
    {
        HideSelection();
    }

    private void OnDestroy()
    {
        if (_dimmer != null)
            Destroy(_dimmer.gameObject);

        if (_highlight != null)
            Destroy(_highlight.gameObject);

        if (_dimmerSprite != null)
            Destroy(_dimmerSprite);
    }
}
