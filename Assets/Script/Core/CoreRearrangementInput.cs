
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class CoreRearrangementInput : MonoBehaviour
{
    private GameStateManager _gameStateManager;
    private CoreManager _coreManager;
    private TowerManager _towerManager;
    private CoreVisual _coreVisual;
    private CoreRearrangementVisual _rearrangementVisual;
    private Camera _mainCamera;

    private InputAction _pressAction;
    private readonly List<RaycastResult> _uiResults = new();

    private Pointer _dragPointer;
    private int _fromSlotNumber;

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
        if (_dragPointer == null)
            return;

        if (_gameStateManager.CurrentState != GameState.Rearrangement)
        {
            ClearDrag();
            return;
        }

        Vector2 screenPosition = _dragPointer.position.ReadValue();
        Vector3 worldPosition = ScreenToWorld(screenPosition);

        _rearrangementVisual.SetDragPosition(worldPosition);
    }

    private void HandlePress(InputAction.CallbackContext context)
    {
        if (_gameStateManager.CurrentState != GameState.Rearrangement)
            return;

        if (context.control.device is not Pointer pointer)
            return;

        if (_dragPointer != null)
            return;

        Vector2 screenPosition = pointer.position.ReadValue();

        if (IsPointerOverUI(screenPosition))
            return;

        if (!TryGetSlotNumber(screenPosition, out int slotNumber))
            return;

        if (!_coreManager.TryGetTower(slotNumber, out Tower tower))
            return;

        _dragPointer = pointer;
        _fromSlotNumber = slotNumber;

        _rearrangementVisual.ShowSelection(tower);
        _rearrangementVisual.SetDragPosition(ScreenToWorld(screenPosition));

        Debug.Log($"재배치 드래그 시작: {slotNumber}");
    }

    private void HandleRelease(InputAction.CallbackContext context)
    {
        if (_dragPointer == null || context.control.device != _dragPointer)
            return;

        int fromSlotNumber = _fromSlotNumber;
        Vector2 screenPosition = _dragPointer.position.ReadValue();

        ClearDrag();

        if (_gameStateManager.CurrentState != GameState.Rearrangement)
            return;

        if (IsPointerOverUI(screenPosition))
            return;

        if (!TryGetSlotNumber(screenPosition, out int toSlotNumber))
            return;

        if (fromSlotNumber == toSlotNumber)
            return;

        if (_towerManager.TryMoveOrSwapTower(fromSlotNumber, toSlotNumber))
            Debug.Log($"재배치 완료: {fromSlotNumber} → {toSlotNumber}");
    }

    private bool TryGetSlotNumber(Vector2 screenPosition, out int slotNumber)
    {
        Vector3 worldPosition = ScreenToWorld(screenPosition);
        return _coreVisual.TryGetSlotNumber(worldPosition, out slotNumber);
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
        ClearDrag();

        if (enabled && isActiveAndEnabled)
            _pressAction.Enable();
        else
            _pressAction.Disable();
    }

    private void ClearDrag()
    {
        _dragPointer = null;
        _fromSlotNumber = 0;

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
        ClearDrag();
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
