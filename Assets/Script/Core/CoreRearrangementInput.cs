
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class CoreRearrangementInput : MonoBehaviour
{
    [SerializeField, Min(5f)]
    private float _dragThreshold = 20f;

    private GameStateManager _gameStateManager;
    private CoreManager _coreManager;
    private TowerManager _towerManager;
    private CoreVisual _coreVisual;
    private CoreRearrangementVisual _rearrangementVisual;
    private Camera _mainCamera;

    private InputAction _pressAction;
    private readonly List<RaycastResult> _uiResults = new();

    private Pointer _activePointer;
    private Vector2 _pressScreenPosition;

    private int _selectedSlotNumber;
    private int _pressSlotNumber;

    private bool _startedSelectionThisPress;
    private bool _isDragging;

    private void Awake()
    {
        _pressAction = new InputAction(
            "RearrangementPress",
            InputActionType.Button,
            "<Pointer>/press"
        );

        _pressAction.performed += HandlePress;
        _pressAction.canceled += HandleRelease;
    }

    public void Initialize(
        GameStateManager gameStateManager,
        CoreManager coreManager,
        TowerManager towerManager,
        CoreVisual coreVisual,
        Camera mainCamera)
    {
        _gameStateManager = gameStateManager;
        _coreManager = coreManager;
        _towerManager = towerManager;
        _coreVisual = coreVisual;
        _mainCamera = mainCamera;

        _rearrangementVisual = GetComponent<CoreRearrangementVisual>();

        if (_rearrangementVisual == null)
            _rearrangementVisual = gameObject.AddComponent<CoreRearrangementVisual>();

        _rearrangementVisual.Initialize(_mainCamera);

        _gameStateManager.StateChanged += HandleStateChanged;

        SetInputEnabled(_gameStateManager.CurrentState == GameState.Rearrangement);
    }

    private void Update()
    {
        if (_selectedSlotNumber == 0)
            return;

        if (_gameStateManager.CurrentState != GameState.Rearrangement)
        {
            ClearSelection();
            return;
        }

        Pointer pointer = _activePointer ?? Pointer.current;

        if (pointer == null)
            return;

        Vector2 screenPosition = pointer.position.ReadValue();

        if (_activePointer != null &&
            _pressSlotNumber == _selectedSlotNumber &&
            !_isDragging &&
            HasMovedEnough(screenPosition))
        {
            _isDragging = true;
            Debug.Log("재배치 드래그 시작");
        }

        _rearrangementVisual.SetDragPosition(ScreenToWorld(screenPosition));
    }

    private void HandlePress(InputAction.CallbackContext context)
    {
        if (_gameStateManager.CurrentState != GameState.Rearrangement)
            return;

        if (context.control.device is not Pointer pointer)
            return;

        if (_activePointer != null)
            return;

        Vector2 screenPosition = pointer.position.ReadValue();

        if (IsPointerOverUI(screenPosition))
            return;

        bool hasSlot = TryGetSlotNumber(screenPosition, out int slotNumber);

        _startedSelectionThisPress = false;

        if (_selectedSlotNumber == 0)
        {
            if (!hasSlot || !_coreManager.TryGetTower(slotNumber, out Tower tower))
                return;

            _selectedSlotNumber = slotNumber;
            _startedSelectionThisPress = true;

            _rearrangementVisual.ShowSelection(tower);
            _rearrangementVisual.SetDragPosition(ScreenToWorld(screenPosition));

            Debug.Log($"재배치 타워 선택: {slotNumber}");
        }

        _activePointer = pointer;
        _pressScreenPosition = screenPosition;
        _pressSlotNumber = hasSlot ? slotNumber : 0;
        _isDragging = false;
    }

    private void HandleRelease(InputAction.CallbackContext context)
    {
        if (_activePointer == null || context.control.device != _activePointer)
            return;

        Vector2 screenPosition = _activePointer.position.ReadValue();

        bool wasNewSelection = _startedSelectionThisPress;

        bool releasedAfterDrag =
            _pressSlotNumber == _selectedSlotNumber &&
            (_isDragging || HasMovedEnough(screenPosition));

        int pressedSlotNumber = _pressSlotNumber;
        int selectedSlotNumber = _selectedSlotNumber;

        ResetPress();

        if (_gameStateManager.CurrentState != GameState.Rearrangement)
        {
            ClearSelection();
            return;
        }

        if (releasedAfterDrag)
        {
            if (!IsPointerOverUI(screenPosition) &&
                TryGetSlotNumber(screenPosition, out int targetSlotNumber))
            {
                TryMoveOrSwap(selectedSlotNumber, targetSlotNumber);
            }

            ClearSelection();
            return;
        }

        if (wasNewSelection)
            return;

        if (pressedSlotNumber == 0 || pressedSlotNumber == selectedSlotNumber)
        {
            ClearSelection();
            return;
        }

        if (IsPointerOverUI(screenPosition))
            return;

        if (!TryGetSlotNumber(screenPosition, out int tappedSlotNumber))
            return;

        if (tappedSlotNumber != pressedSlotNumber)
            return;

        TryMoveOrSwap(selectedSlotNumber, tappedSlotNumber);
        ClearSelection();
    }

    private void TryMoveOrSwap(int fromSlotNumber, int toSlotNumber)
    {
        if (fromSlotNumber == toSlotNumber)
            return;

        if (_towerManager.TryMoveOrSwapTower(fromSlotNumber, toSlotNumber))
            Debug.Log($"재배치 완료: {fromSlotNumber} → {toSlotNumber}");
    }

    private bool HasMovedEnough(Vector2 screenPosition)
    {
        float threshold = _dragThreshold * _dragThreshold;

        return (screenPosition - _pressScreenPosition).sqrMagnitude >= threshold;
    }

    private bool TryGetSlotNumber(Vector2 screenPosition, out int slotNumber)
    {
        return _coreVisual.TryGetSlotNumber(
            ScreenToWorld(screenPosition),
            out slotNumber
        );
    }

    private Vector3 ScreenToWorld(Vector2 screenPosition)
    {
        float depth = Mathf.Abs(
            _coreVisual.transform.position.z - _mainCamera.transform.position.z
        );

        return _mainCamera.ScreenToWorldPoint(
            new Vector3(screenPosition.x, screenPosition.y, depth)
        );
    }

    private void HandleStateChanged(GameState previousState, GameState nextState)
    {
        SetInputEnabled(nextState == GameState.Rearrangement);
    }

    private void SetInputEnabled(bool enabled)
    {
        ClearSelection();

        if (enabled && isActiveAndEnabled)
            _pressAction.Enable();
        else
            _pressAction.Disable();
    }

    private void ResetPress()
    {
        _activePointer = null;
        _pressSlotNumber = 0;
        _startedSelectionThisPress = false;
        _isDragging = false;
    }

    private void ClearSelection()
    {
        ResetPress();
        _selectedSlotNumber = 0;

        if (_rearrangementVisual != null)
            _rearrangementVisual.HideSelection();
    }

    private bool IsPointerOverUI(Vector2 screenPosition)
    {
        if (EventSystem.current == null)
            return false;

        PointerEventData eventData = new(EventSystem.current)
        {
            position = screenPosition
        };

        _uiResults.Clear();
        EventSystem.current.RaycastAll(eventData, _uiResults);

        return _uiResults.Count > 0;
    }

    private void OnEnable()
    {
        if (_gameStateManager != null)
            SetInputEnabled(_gameStateManager.CurrentState == GameState.Rearrangement);
    }

    private void OnDisable()
    {
        ClearSelection();
        _pressAction?.Disable();
    }

    private void OnDestroy()
    {
        if (_gameStateManager != null)
            _gameStateManager.StateChanged -= HandleStateChanged;

        if (_pressAction == null)
            return;

        _pressAction.performed -= HandlePress;
        _pressAction.canceled -= HandleRelease;
        _pressAction.Dispose();
    }
}
