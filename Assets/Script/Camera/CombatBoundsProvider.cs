using System;
using UnityEngine;

public sealed class CombatBoundsProvider
{
    private readonly Camera _worldCamera;
    private readonly CameraBoundsProvider _cameraBoundsProvider;
    private readonly RectTransform _reservedBottomPanel;
    private readonly Canvas _canvas;
    private readonly Vector3[] _worldCorners = new Vector3[4];

    public CombatBoundsProvider(
        Camera worldCamera,
        RectTransform reservedBottomPanel)
    {
        _worldCamera = worldCamera != null
            ? worldCamera
            : throw new ArgumentNullException(nameof(worldCamera));
        _reservedBottomPanel = reservedBottomPanel != null
            ? reservedBottomPanel
            : throw new ArgumentNullException(nameof(reservedBottomPanel));
        _canvas = reservedBottomPanel.GetComponentInParent<Canvas>();
        _cameraBoundsProvider = new CameraBoundsProvider(worldCamera);

        if (_canvas == null)
            throw new InvalidOperationException("The reserved combat UI panel must be inside a Canvas.");
    }

    public CombatBounds GetBounds()
    {
        CameraBounds cameraBounds = _cameraBoundsProvider.GetBounds();
        float panelTopScreenY = GetPanelTopScreenY();
        Rect pixelRect = _worldCamera.pixelRect;
        float bottomViewportY = pixelRect.height > 0f
            ? Mathf.Clamp01((panelTopScreenY - pixelRect.yMin) / pixelRect.height)
            : 0f;
        return CombatBounds.FromCameraBounds(cameraBounds, bottomViewportY);
    }

    private float GetPanelTopScreenY()
    {
        _reservedBottomPanel.GetWorldCorners(_worldCorners);
        Canvas rootCanvas = _canvas.rootCanvas;
        Camera uiCamera = rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay
            ? null
            : rootCanvas.worldCamera;
        float top = float.NegativeInfinity;

        for (int i = 0; i < _worldCorners.Length; i++)
        {
            Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(uiCamera, _worldCorners[i]);
            top = Mathf.Max(top, screenPoint.y);
        }

        return top;
    }
}
