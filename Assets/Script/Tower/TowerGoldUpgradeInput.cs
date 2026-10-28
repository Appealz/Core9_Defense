using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public sealed class TowerGoldUpgradeInput : MonoBehaviour
{
    private readonly List<RaycastResult> _uiResults = new();
    private GameStateManager _gameStateManager;
    private CoreManager _coreManager;
    private CoreVisual _coreVisual;
    private TowerSelectionManager _selectionManager;
    private Camera _mainCamera;
    private InputAction _pressAction;

    private void Awake()
    {
        _pressAction = new InputAction(
            "TowerGoldUpgradeSelect",
            InputActionType.Button,
            "<Pointer>/press");
        _pressAction.performed += HandlePress;
    }

    public void Initialize(
        GameStateManager gameStateManager,
        CoreManager coreManager,
        CoreVisual coreVisual,
        TowerSelectionManager selectionManager,
        Camera mainCamera)
    {
        _gameStateManager = gameStateManager;
        _coreManager = coreManager;
        _coreVisual = coreVisual;
        _selectionManager = selectionManager;
        _mainCamera = mainCamera;
        _gameStateManager.StateChanged += HandleStateChanged;
        SetInputEnabled(_gameStateManager.CurrentState == GameState.Playing);
    }

    private void HandlePress(InputAction.CallbackContext context)
    {
        if (_gameStateManager == null || _gameStateManager.CurrentState != GameState.Playing ||
            context.control.device is not Pointer pointer)
        {
            return;
        }

        Vector2 screenPosition = pointer.position.ReadValue();

        if (IsPointerOverUI(screenPosition))
            return;

        if (_coreVisual.TryGetSlotNumber(ScreenToWorld(screenPosition), out int slotNumber) &&
            _coreManager.TryGetTower(slotNumber, out Tower tower))
        {
            _selectionManager.TrySelect(tower);
            return;
        }

        _selectionManager.ClearSelection();
    }

    private Vector3 ScreenToWorld(Vector2 screenPosition)
    {
        float depth = Mathf.Abs(
            _coreVisual.transform.position.z - _mainCamera.transform.position.z);

        return _mainCamera.ScreenToWorldPoint(
            new Vector3(screenPosition.x, screenPosition.y, depth));
    }

    private bool IsPointerOverUI(Vector2 screenPosition)
    {
        if (EventSystem.current == null)
            return false;

        PointerEventData eventData = new(EventSystem.current) { position = screenPosition };
        _uiResults.Clear();
        EventSystem.current.RaycastAll(eventData, _uiResults);
        return _uiResults.Count > 0;
    }

    private void HandleStateChanged(GameState _, GameState nextState)
    {
        SetInputEnabled(nextState == GameState.Playing);
    }

    private void SetInputEnabled(bool enabled)
    {
        if (enabled && isActiveAndEnabled)
            _pressAction.Enable();
        else
            _pressAction.Disable();
    }

    private void OnEnable()
    {
        if (_gameStateManager != null)
            SetInputEnabled(_gameStateManager.CurrentState == GameState.Playing);
    }

    private void OnDisable()
    {
        _pressAction?.Disable();
    }

    private void OnDestroy()
    {
        if (_gameStateManager != null)
            _gameStateManager.StateChanged -= HandleStateChanged;

        if (_pressAction == null)
            return;

        _pressAction.performed -= HandlePress;
        _pressAction.Dispose();
    }
}
