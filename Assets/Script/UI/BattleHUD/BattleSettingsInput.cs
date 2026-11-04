using System;
using UnityEngine;
using UnityEngine.InputSystem;

public sealed class BattleSettingsInput : MonoBehaviour
{
    private InputAction _toggleAction;
    private bool _isInitialized;

    public event Action ToggleRequested;

    public void Initialize()
    {
        if (_isInitialized)
            return;

        _toggleAction = new InputAction(
            "ToggleBattleSettings",
            InputActionType.Button,
            "<Keyboard>/escape");
        _toggleAction.performed += HandleTogglePerformed;
        _isInitialized = true;

        if (isActiveAndEnabled)
            _toggleAction.Enable();
    }

    private void OnEnable()
    {
        if (_isInitialized)
            _toggleAction.Enable();
    }

    private void OnDisable()
    {
        if (_isInitialized)
            _toggleAction.Disable();
    }

    private void HandleTogglePerformed(InputAction.CallbackContext context)
    {
        if (context.performed)
            ToggleRequested?.Invoke();
    }

    private void OnDestroy()
    {
        if (!_isInitialized)
            return;

        _toggleAction.performed -= HandleTogglePerformed;
        _toggleAction.Disable();
        _toggleAction.Dispose();
        _toggleAction = null;
        _isInitialized = false;
    }
}
