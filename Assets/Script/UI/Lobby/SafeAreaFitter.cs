using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public sealed class SafeAreaFitter : MonoBehaviour
{
    private RectTransform _rectTransform;
    private Rect _lastSafeArea;
    private Vector2Int _lastScreenSize;

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        Apply();
    }

    private void Update()
    {
        if (_lastSafeArea != Screen.safeArea ||
            _lastScreenSize.x != Screen.width ||
            _lastScreenSize.y != Screen.height)
        {
            Apply();
        }
    }

    private void Apply()
    {
        if (_rectTransform == null)
            _rectTransform = GetComponent<RectTransform>();

        Rect safeArea = Screen.safeArea;
        float width = Mathf.Max(1f, Screen.width);
        float height = Mathf.Max(1f, Screen.height);
        _rectTransform.anchorMin = new Vector2(safeArea.xMin / width, safeArea.yMin / height);
        _rectTransform.anchorMax = new Vector2(safeArea.xMax / width, safeArea.yMax / height);
        _rectTransform.offsetMin = Vector2.zero;
        _rectTransform.offsetMax = Vector2.zero;
        _lastSafeArea = safeArea;
        _lastScreenSize = new Vector2Int(Screen.width, Screen.height);
    }
}
