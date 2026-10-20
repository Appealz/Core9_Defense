
using System;
using UnityEngine;

public class CoreVisual : MonoBehaviour
{
    [SerializeField] private SpriteRenderer _slotPrefab;
    [SerializeField] private Color _hitTargetColor = new(1f, 0.2f, 0.2f, 0.65f);

    private CoreLayout _coreLayout;
    private CoreManager _coreManager;
    private SpriteRenderer[] _slots;
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
            _coreManager.HitTargetSlotChanged -= HandleHitTargetSlotChanged;

        _coreManager = coreManager ?? throw new ArgumentNullException(nameof(coreManager));
        _coreManager.HitTargetSlotChanged += HandleHitTargetSlotChanged;

        _coreManager.TryFindHitTargetSlotNumber(out int slotNumber);
        SetHitTargetSlot(slotNumber);
    }

    private void CreateSlots()
    {
        _normalColor = _slotPrefab.color;
        _slots = new SpriteRenderer[_coreLayout.SlotCount + 1];

        for (int slotNumber = 1; slotNumber <= _coreLayout.SlotCount; slotNumber++)
        {
            Vector3 position = _coreLayout.GetSlotPosition(slotNumber);

            SpriteRenderer slot = Instantiate(_slotPrefab, position, Quaternion.identity, transform);
            slot.name = $"CoreSlotVisual_{slotNumber}";

            _slots[slotNumber] = slot;
        }
    }

    private void HandleHitTargetSlotChanged(int slotNumber)
    {
        SetHitTargetSlot(slotNumber);
    }

    private void SetHitTargetSlot(int slotNumber)
    {
        if (_currentHitTargetSlotNumber == slotNumber)
            return;

        if (_currentHitTargetSlotNumber > 0)
            _slots[_currentHitTargetSlotNumber].color = _normalColor;

        _currentHitTargetSlotNumber = slotNumber;

        if (slotNumber > 0)
            _slots[slotNumber].color = _hitTargetColor;
    }

    private void OnDestroy()
    {
        if (_coreManager != null)
            _coreManager.HitTargetSlotChanged -= HandleHitTargetSlotChanged;
    }
}
