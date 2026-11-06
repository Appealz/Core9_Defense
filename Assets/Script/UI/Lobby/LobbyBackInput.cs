using System;
using UnityEngine;
using UnityEngine.InputSystem;

public sealed class LobbyBackInput : MonoBehaviour
{
    private InputAction _backAction;
    private bool _isInitialized;

    public event Action BackRequested;

    public void Initialize()
    {
        if (_isInitialized)
            return;

        _backAction = new InputAction(
            "LobbyBack",
            InputActionType.Button,
            "<Keyboard>/escape");
        _backAction.performed += HandleBackPerformed;
        _isInitialized = true;

        if (isActiveAndEnabled)
            _backAction.Enable();
    }

    private void OnEnable()
    {
        if (_isInitialized)
            _backAction.Enable();
    }

    private void OnDisable()
    {
        if (_isInitialized)
            _backAction.Disable();
    }

    private void HandleBackPerformed(InputAction.CallbackContext context)
    {
        if (context.performed)
            BackRequested?.Invoke();
    }

    private void OnDestroy()
    {
        if (!_isInitialized)
            return;

        _backAction.performed -= HandleBackPerformed;
        _backAction.Disable();
        _backAction.Dispose();
        _backAction = null;
        _isInitialized = false;
    }
}
