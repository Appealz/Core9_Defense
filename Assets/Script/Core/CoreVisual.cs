using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CoreVisual : MonoBehaviour
{
    [SerializeField] private SpriteRenderer _slotPrefab;

    [Header("Hit Target Border")]
    [SerializeField] private Color _hitTargetColor = new(1f, 0.15f, 0.15f, 0.65f);
    [Tooltip("World-space thickness of each hit target border edge.")]
    [SerializeField, Min(0.001f)] private float _hitTargetThickness = 0.06f;

    [Header("Enhancement Preview")]
    [SerializeField] private Color _validPreviewColor = new(0.2f, 1f, 0.25f, 0.65f);
    [SerializeField] private Color _invalidPreviewColor = new(1f, 0.15f, 0.15f, 0.65f);
    [SerializeField, Range(0.1f, 1.5f)] private float _previewScale = 0.9f;

    [Header("Effect Count")]
    [SerializeField] private Color _effectCountColor = Color.white;
    [SerializeField, Min(0.1f)] private float _effectCountFontSize = 2f;

    private CoreLayout _coreLayout;
    private CoreManager _coreManager;
    private SpriteRenderer[] _slots;
    private GameObject[] _hitTargetBorders;
    private readonly List<SpriteRenderer> _previewGuides = new();
    private TextMeshPro[] _effectCounts;
    private Sprite _hitTargetBorderSprite;
    private Color _normalColor;
    private int _currentHitTargetSlotNumber;

    private void Awake()
    {
        _coreLayout = GetComponentInParent<CoreLayout>();

        if (_coreLayout == null || _slotPrefab == null)
        {
            Debug.LogError("CoreVisual 초기화에 필요한 참조가 없습니다.", this);
            return;
        }

        CreateSlots();
    }

    public void Initialize(CoreManager coreManager)
    {
        if (_slots == null)
            throw new InvalidOperationException("CoreVisual 슬롯이 생성되지 않았습니다.");

        if (_coreManager != null)
        {
            _coreManager.HitTargetSlotChanged -= HandleHitTargetSlotChanged;
            _coreManager.SlotEffectsChanged -= HandleSlotEffectsChanged;
        }

        _coreManager = coreManager ?? throw new ArgumentNullException(nameof(coreManager));
        _coreManager.HitTargetSlotChanged += HandleHitTargetSlotChanged;
        _coreManager.SlotEffectsChanged += HandleSlotEffectsChanged;

        for (int slotNumber = 1; slotNumber <= _coreLayout.SlotCount; slotNumber++)
            RefreshSlotEnhancement(slotNumber);

        _coreManager.TryFindHitTargetSlotNumber(out int hitTargetSlotNumber);
        SetHitTargetSlot(hitTargetSlotNumber);
    }

    public bool TryGetSlotNumber(Vector2 worldPosition, out int slotNumber)
    {
        slotNumber = 0;

        if (_slots == null)
            return false;

        float closestDistance = float.MaxValue;

        for (int i = 1; i < _slots.Length; i++)
        {
            Bounds bounds = _slots[i].bounds;

            if (worldPosition.x < bounds.min.x || worldPosition.x > bounds.max.x ||
                worldPosition.y < bounds.min.y || worldPosition.y > bounds.max.y)
            {
                continue;
            }

            float distance = ((Vector2)bounds.center - worldPosition).sqrMagnitude;

            if (distance >= closestDistance)
                continue;

            closestDistance = distance;
            slotNumber = i;
        }

        return slotNumber != 0;
    }

    public void ShowEnhancementPreview(IReadOnlyList<Vector2Int> gridCells, bool isValid)
    {
        ClearEnhancementPreview();

        if (gridCells == null)
            return;

        EnsurePreviewGuideCount(gridCells.Count);
        Color guideColor = isValid ? _validPreviewColor : _invalidPreviewColor;

        for (int i = 0; i < gridCells.Count; i++)
        {
            SpriteRenderer guide = _previewGuides[i];
            guide.transform.position = _coreLayout.GetGridPosition(gridCells[i]);
            guide.color = guideColor;
            guide.enabled = true;
        }
    }

    public void ClearEnhancementPreview()
    {
        for (int i = 0; i < _previewGuides.Count; i++)
            _previewGuides[i].enabled = false;
    }

    private void CreateSlots()
    {
        _normalColor = _slotPrefab.color;
        int arraySize = _coreLayout.SlotCount + 1;
        _slots = new SpriteRenderer[arraySize];
        _hitTargetBorders = new GameObject[arraySize];
        _effectCounts = new TextMeshPro[arraySize];

        Texture2D whiteTexture = Texture2D.whiteTexture;
        _hitTargetBorderSprite = Sprite.Create(
            whiteTexture,
            new Rect(0f, 0f, whiteTexture.width, whiteTexture.height),
            new Vector2(0.5f, 0.5f),
            whiteTexture.width
        );
        _hitTargetBorderSprite.name = "RuntimeHitTargetBorderSprite";

        for (int slotNumber = 1; slotNumber <= _coreLayout.SlotCount; slotNumber++)
        {
            Vector3 position = _coreLayout.GetSlotPosition(slotNumber);
            SpriteRenderer slot = Instantiate(_slotPrefab, position, Quaternion.identity, transform);
            slot.name = $"CoreSlotVisual_{slotNumber}";
            _slots[slotNumber] = slot;

            _hitTargetBorders[slotNumber] = CreateHitTargetBorder(slot);
            _effectCounts[slotNumber] = CreateEffectCount(slot);
        }
    }

    private GameObject CreateHitTargetBorder(SpriteRenderer source)
    {
        GameObject borderRoot = new GameObject("HitTargetBorder");
        borderRoot.transform.SetParent(source.transform, false);

        Bounds bounds = source.localBounds;
        float worldScaleX = Mathf.Max(0.0001f, Mathf.Abs(source.transform.lossyScale.x));
        float worldScaleY = Mathf.Max(0.0001f, Mathf.Abs(source.transform.lossyScale.y));
        float worldThickness = Mathf.Max(0.001f, _hitTargetThickness);
        float localThicknessX = worldThickness / worldScaleX;
        float localThicknessY = worldThickness / worldScaleY;
        float horizontalWidth = bounds.size.x + localThicknessX * 2f;

        CreateBorderEdge(
            borderRoot.transform,
            "Top",
            new Vector3(bounds.center.x, bounds.max.y + localThicknessY * 0.5f, 0f),
            new Vector3(horizontalWidth, localThicknessY, 1f),
            source
        );
        CreateBorderEdge(
            borderRoot.transform,
            "Bottom",
            new Vector3(bounds.center.x, bounds.min.y - localThicknessY * 0.5f, 0f),
            new Vector3(horizontalWidth, localThicknessY, 1f),
            source
        );
        CreateBorderEdge(
            borderRoot.transform,
            "Left",
            new Vector3(bounds.min.x - localThicknessX * 0.5f, bounds.center.y, 0f),
            new Vector3(localThicknessX, bounds.size.y, 1f),
            source
        );
        CreateBorderEdge(
            borderRoot.transform,
            "Right",
            new Vector3(bounds.max.x + localThicknessX * 0.5f, bounds.center.y, 0f),
            new Vector3(localThicknessX, bounds.size.y, 1f),
            source
        );

        borderRoot.SetActive(false);
        return borderRoot;
    }

    private void CreateBorderEdge(
        Transform parent,
        string edgeName,
        Vector3 localPosition,
        Vector3 localScale,
        SpriteRenderer source)
    {
        GameObject edgeObject = new GameObject(edgeName);
        edgeObject.transform.SetParent(parent, false);
        edgeObject.transform.localPosition = localPosition;
        edgeObject.transform.localScale = localScale;

        SpriteRenderer edge = edgeObject.AddComponent<SpriteRenderer>();
        edge.sprite = _hitTargetBorderSprite;
        edge.color = _hitTargetColor;
        edge.sortingLayerID = source.sortingLayerID;
        edge.sortingOrder = source.sortingOrder + 2;
    }

    private void EnsurePreviewGuideCount(int requiredCount)
    {
        while (_previewGuides.Count < requiredCount)
        {
            GameObject guideObject = new GameObject($"EnhancementPreview_{_previewGuides.Count + 1}");
            guideObject.transform.SetParent(transform, false);
            guideObject.transform.localScale = _slotPrefab.transform.localScale * _previewScale;

            SpriteRenderer guide = guideObject.AddComponent<SpriteRenderer>();
            guide.sprite = _slotPrefab.sprite;
            guide.drawMode = _slotPrefab.drawMode;
            guide.size = _slotPrefab.size;
            guide.flipX = _slotPrefab.flipX;
            guide.flipY = _slotPrefab.flipY;
            guide.sortingLayerID = _slotPrefab.sortingLayerID;
            guide.sortingOrder = _slotPrefab.sortingOrder + 3;
            guide.enabled = false;
            _previewGuides.Add(guide);
        }
    }

    private TextMeshPro CreateEffectCount(SpriteRenderer slot)
    {
        GameObject countObject = new GameObject("EffectCount");
        countObject.transform.SetParent(slot.transform, false);
        countObject.transform.localPosition = new Vector3(0.32f, 0.32f, 0f);

        TextMeshPro count = countObject.AddComponent<TextMeshPro>();
        count.alignment = TextAlignmentOptions.Center;
        count.fontSize = _effectCountFontSize;
        count.color = _effectCountColor;
        count.sortingLayerID = slot.sortingLayerID;
        count.sortingOrder = slot.sortingOrder + 4;
        count.text = string.Empty;
        return count;
    }

    private void HandleHitTargetSlotChanged(int slotNumber)
    {
        SetHitTargetSlot(slotNumber);
    }

    private void HandleSlotEffectsChanged(int slotNumber)
    {
        RefreshSlotEnhancement(slotNumber);
    }

    private void SetHitTargetSlot(int slotNumber)
    {
        if (_currentHitTargetSlotNumber == slotNumber)
            return;

        if (_currentHitTargetSlotNumber > 0)
            _hitTargetBorders[_currentHitTargetSlotNumber].SetActive(false);

        _currentHitTargetSlotNumber = slotNumber;

        if (slotNumber > 0)
            _hitTargetBorders[slotNumber].SetActive(true);
    }

    private void RefreshSlotEnhancement(int slotNumber)
    {
        if (!_coreManager.TryGetSlotEffects(slotNumber, out IReadOnlyList<CoreSlotEffect> effects))
            return;

        _slots[slotNumber].color = GetPrimaryEffectColor(effects, _normalColor);
        _effectCounts[slotNumber].text = effects.Count > 1 ? effects.Count.ToString() : string.Empty;
    }

    private static Color GetPrimaryEffectColor(
        IReadOnlyList<CoreSlotEffect> effects,
        Color fallbackColor)
    {
        CoreSlotEffect primaryEffect = null;

        for (int i = 0; i < effects.Count; i++)
        {
            CoreSlotEffect effect = effects[i];

            if (primaryEffect == null ||
                effect.VisualPriority > primaryEffect.VisualPriority ||
                effect.VisualPriority == primaryEffect.VisualPriority &&
                string.CompareOrdinal(effect.EffectId, primaryEffect.EffectId) < 0)
            {
                primaryEffect = effect;
            }
        }

        return primaryEffect?.SlotColor ?? fallbackColor;
    }

    private void OnDestroy()
    {
        if (_coreManager != null)
        {
            _coreManager.HitTargetSlotChanged -= HandleHitTargetSlotChanged;
            _coreManager.SlotEffectsChanged -= HandleSlotEffectsChanged;
        }

        if (_hitTargetBorderSprite != null)
            Destroy(_hitTargetBorderSprite);
    }
}
