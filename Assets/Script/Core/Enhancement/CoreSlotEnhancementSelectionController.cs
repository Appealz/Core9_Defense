using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public sealed class CoreSlotEnhancementSelectionController : MonoBehaviour
{
    [Header("Runtime UI")]
    [SerializeField] private Color _confirmButtonColor = new(0.2f, 0.7f, 0.3f, 0.95f);
    [SerializeField] private Color _instructionColor = Color.white;

    private readonly List<int> _candidateSlots = new();
    private readonly List<int> _fixedTargetSlots = new();
    private readonly List<Vector2Int> _previewGridCells = new();
    private readonly List<RaycastResult> _uiResults = new();

    private GameStateManager _gameStateManager;
    private CoreManager _coreManager;
    private CoreVisual _coreVisual;
    private Camera _mainCamera;
    private InputAction _pressAction;

    private CoreSlotPatternData _pendingPattern;
    private IReadOnlyList<CoreSlotEffect> _pendingEffects;
    private UniTaskCompletionSource<bool> _completionSource;
    private int _fixedAnchorSlotNumber;
    private bool _isSelecting;
    private bool _hasFixedSelection;
    private bool _isFixedPlacementValid;
    private bool _isConfirming;

    private Canvas _selectionCanvas;
    private Button _confirmButton;
    private Text _instructionText;

    private void Awake()
    {
        _pressAction = new InputAction("CoreSlotEnhancementPress", InputActionType.Button, "<Pointer>/press");
        _pressAction.performed += HandlePress;
    }

    public void Initialize(
        GameStateManager gameStateManager,
        CoreManager coreManager,
        CoreVisual coreVisual,
        Camera mainCamera)
    {
        _gameStateManager = gameStateManager;
        _coreManager = coreManager;
        _coreVisual = coreVisual;
        _mainCamera = mainCamera;

        EnsureSelectionCanvas();
        HideSelectionUi();
    }

    public bool HasValidPlacement(
        CoreSlotPatternData pattern,
        IReadOnlyList<CoreSlotEffect> effects)
    {
        if (pattern == null || effects == null || effects.Count == 0 || _coreManager == null)
            return false;

        for (int anchorSlotNumber = 1; anchorSlotNumber <= CoreBoard.SlotCount; anchorSlotNumber++)
        {
            if (TryGetValidSlots(pattern, effects, anchorSlotNumber, _candidateSlots))
                return true;
        }

        return false;
    }

    public async UniTask<bool> SelectAndApplyAsync(
        CoreSlotPatternData pattern,
        IReadOnlyList<CoreSlotEffect> effects)
    {
        if (_isSelecting || !HasValidPlacement(pattern, effects))
            return false;

        _pendingPattern = pattern;
        _pendingEffects = effects;
        _completionSource = new UniTaskCompletionSource<bool>();
        _isSelecting = true;
        ClearPreview();

        EnsureSelectionCanvas();
        _instructionText.text = $"{pattern.PatternName} 강화 위치를 선택하세요";
        _selectionCanvas.gameObject.SetActive(true);
        _confirmButton.interactable = false;
        _pressAction.Enable();
        _gameStateManager.ChangeState(GameState.CoreSlotSelection);

        return await _completionSource.Task;
    }

    private void HandlePress(InputAction.CallbackContext context)
    {
        if (!_isSelecting || _gameStateManager.CurrentState != GameState.CoreSlotSelection)
            return;

        if (context.control.device is not Pointer pointer)
            return;

        Vector2 screenPosition = pointer.position.ReadValue();

        if (IsPointerOverUi(screenPosition))
            return;

        UpdatePreview(screenPosition, true);
    }

    private void Update()
    {
        if (!_isSelecting ||
            _hasFixedSelection ||
            _gameStateManager.CurrentState != GameState.CoreSlotSelection)
        {
            return;
        }

        Mouse mouse = Mouse.current;

        if (mouse == null)
            return;

        Vector2 screenPosition = mouse.position.ReadValue();

        if (!IsPointerOverUi(screenPosition))
            UpdatePreview(screenPosition, false);
    }

    private void UpdatePreview(Vector2 screenPosition, bool fixSelection)
    {
        if (!_coreVisual.TryGetSlotNumber(ScreenToWorld(screenPosition), out int anchorSlotNumber))
            return;

        if (!_pendingPattern.TryGetGridCells(
                anchorSlotNumber,
                CoreBoard.GridSize,
                _previewGridCells,
                out bool allInside))
        {
            ClearPreview();
            return;
        }

        _candidateSlots.Clear();
        bool canConfirm = allInside &&
            _pendingPattern.TryGetSlotNumbers(
                anchorSlotNumber,
                CoreBoard.GridSize,
                _candidateSlots) &&
            _coreManager.CanAddSlotEffects(_candidateSlots, _pendingEffects);

        if (!allInside)
            _candidateSlots.Clear();

        _coreVisual.ShowEnhancementPreview(_previewGridCells, canConfirm);

        if (!fixSelection)
        {
            _confirmButton.interactable = false;
            return;
        }

        _hasFixedSelection = true;
        _fixedAnchorSlotNumber = anchorSlotNumber;
        _isFixedPlacementValid = canConfirm;
        _fixedTargetSlots.Clear();
        _fixedTargetSlots.AddRange(_candidateSlots);
        _confirmButton.interactable = _isFixedPlacementValid;
    }

    private void ConfirmSelection()
    {
        if (!_isSelecting ||
            !_hasFixedSelection ||
            !_isFixedPlacementValid ||
            _fixedAnchorSlotNumber == 0 ||
            _isConfirming)
        {
            return;
        }

        if (!_coreManager.CanAddSlotEffects(_fixedTargetSlots, _pendingEffects))
        {
            _isFixedPlacementValid = false;
            _confirmButton.interactable = false;
            _coreVisual.ShowEnhancementPreview(_previewGridCells, false);
            return;
        }

        _isConfirming = true;

        if (!_coreManager.TryAddSlotEffects(_fixedTargetSlots, _pendingEffects))
        {
            _isConfirming = false;
            return;
        }

        CompleteSelection(true);
    }

    private bool TryGetValidSlots(
        CoreSlotPatternData pattern,
        IReadOnlyList<CoreSlotEffect> effects,
        int anchorSlotNumber,
        List<int> results)
    {
        return pattern.TryGetSlotNumbers(anchorSlotNumber, CoreBoard.GridSize, results) &&
            _coreManager.CanAddSlotEffects(results, effects);
    }

    private void ClearPreview()
    {
        _fixedAnchorSlotNumber = 0;
        _hasFixedSelection = false;
        _isFixedPlacementValid = false;
        _isConfirming = false;
        _candidateSlots.Clear();
        _fixedTargetSlots.Clear();
        _previewGridCells.Clear();
        _coreVisual.ClearEnhancementPreview();

        if (_confirmButton != null)
            _confirmButton.interactable = false;
    }

    private void CompleteSelection(bool result)
    {
        if (!_isSelecting)
            return;

        UniTaskCompletionSource<bool> completionSource = _completionSource;

        _isSelecting = false;
        _pendingPattern = null;
        _pendingEffects = null;
        _completionSource = null;
        _pressAction.Disable();
        ClearPreview();
        HideSelectionUi();

        completionSource.TrySetResult(result);
    }

    private void EnsureSelectionCanvas()
    {
        if (_selectionCanvas != null)
            return;

        GameObject canvasObject = new GameObject(
            "CoreSlotEnhancementSelectionUI",
            typeof(RectTransform),
            typeof(Canvas),
            typeof(CanvasScaler),
            typeof(GraphicRaycaster)
        );
        canvasObject.transform.SetParent(transform, false);

        _selectionCanvas = canvasObject.GetComponent<Canvas>();
        _selectionCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        _selectionCanvas.sortingOrder = 32010;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080f, 1920f);
        scaler.matchWidthOrHeight = 0.5f;

        _instructionText = CreateText(canvasObject.transform, "Instruction", new Vector2(0f, 650f));
        _instructionText.color = _instructionColor;
        _instructionText.fontSize = 42;
        _instructionText.alignment = TextAnchor.MiddleCenter;

        GameObject buttonObject = new GameObject(
            "ConfirmEnhancementButton",
            typeof(RectTransform),
            typeof(Image),
            typeof(Button)
        );
        buttonObject.transform.SetParent(canvasObject.transform, false);

        RectTransform buttonRect = buttonObject.GetComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(0.5f, 0f);
        buttonRect.anchorMax = new Vector2(0.5f, 0f);
        buttonRect.pivot = new Vector2(0.5f, 0f);
        buttonRect.anchoredPosition = new Vector2(0f, 90f);
        buttonRect.sizeDelta = new Vector2(520f, 120f);

        buttonObject.GetComponent<Image>().color = _confirmButtonColor;
        _confirmButton = buttonObject.GetComponent<Button>();
        _confirmButton.onClick.AddListener(ConfirmSelection);

        Text buttonText = CreateText(buttonObject.transform, "Label", Vector2.zero);
        RectTransform labelRect = buttonText.rectTransform;
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = Vector2.zero;
        labelRect.offsetMax = Vector2.zero;
        buttonText.text = "강화 위치 확정";
        buttonText.fontSize = 38;
        buttonText.alignment = TextAnchor.MiddleCenter;
        buttonText.color = Color.white;
    }

    private static Text CreateText(Transform parent, string objectName, Vector2 anchoredPosition)
    {
        GameObject textObject = new GameObject(objectName, typeof(RectTransform), typeof(Text));
        textObject.transform.SetParent(parent, false);

        Text text = textObject.GetComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        RectTransform rect = text.rectTransform;
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = new Vector2(900f, 120f);

        return text;
    }

    private void HideSelectionUi()
    {
        if (_selectionCanvas != null)
            _selectionCanvas.gameObject.SetActive(false);
    }

    private Vector3 ScreenToWorld(Vector2 screenPosition)
    {
        float depth = Mathf.Abs(_coreVisual.transform.position.z - _mainCamera.transform.position.z);
        return _mainCamera.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, depth));
    }

    private bool IsPointerOverUi(Vector2 screenPosition)
    {
        if (EventSystem.current == null)
            return false;

        PointerEventData eventData = new PointerEventData(EventSystem.current)
        {
            position = screenPosition
        };

        _uiResults.Clear();
        EventSystem.current.RaycastAll(eventData, _uiResults);
        return _uiResults.Count > 0;
    }

    private void OnDisable()
    {
        if (_isSelecting)
            CompleteSelection(false);
        else
            _pressAction?.Disable();
    }

    private void OnDestroy()
    {
        if (_isSelecting)
            CompleteSelection(false);

        if (_confirmButton != null)
            _confirmButton.onClick.RemoveListener(ConfirmSelection);

        if (_pressAction == null)
            return;

        _pressAction.performed -= HandlePress;
        _pressAction.Dispose();
    }
}
