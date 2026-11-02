using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class GameEndView : MonoBehaviour
{
    private static readonly Color PanelColor = new Color(0.045f, 0.055f, 0.075f, 0.98f);
    private static readonly Color TitleColor = new Color(1f, 0.79f, 0.18f, 1f);
    private static readonly Color PrimaryColor = new Color(1f, 0.78f, 0.12f, 1f);
    private static readonly Color SecondaryColor = new Color(0.16f, 0.18f, 0.22f, 1f);
    private static readonly Color MutedTextColor = new Color(0.74f, 0.77f, 0.82f, 1f);

    private GameObject _root;
    private GameObject _gameOverPanel;
    private GameObject _resultPanel;
    private TMP_Text _gameOverStatsText;
    private TMP_Text _resultSummaryText;
    private TMP_Text _rewardAmountText;
    private TMP_Text _totalRewardText;
    private TMP_Text _continueButtonText;
    private TMP_Text _doubleRewardButtonText;
    private Button _continueButton;
    private Button _finishButton;
    private Button _doubleRewardButton;
    private Button _restartButton;
    private Button _mainMenuButton;

    public event Action ContinueClicked;
    public event Action FinishClicked;
    public event Action DoubleRewardClicked;
    public event Action RestartClicked;
    public event Action MainMenuClicked;

    public void Initialize()
    {
        if (_root != null)
            return;

        Build();
        Hide();
    }

    public void ShowGameOver(GameRunSnapshot snapshot, bool canContinue)
    {
        _root.SetActive(true);
        _gameOverPanel.SetActive(true);
        _resultPanel.SetActive(false);
        _gameOverStatsText.text =
            $"Score  {snapshot.Score:N0}     Wave  {snapshot.ReachedWave}     Currency  {snapshot.BaseCurrencyReward:N0}";
        _continueButton.interactable = canContinue;
        _continueButtonText.text = canContinue
            ? "▶  Watch Ad to Continue"
            : "Continue Already Used";
        _finishButton.interactable = true;
    }

    public void ShowResult(GameRunSnapshot snapshot, int totalReward, bool canDoubleReward)
    {
        _root.SetActive(true);
        _gameOverPanel.SetActive(false);
        _resultPanel.SetActive(true);
        _resultSummaryText.text = $"Score {snapshot.Score:N0}  ·  Wave {snapshot.ReachedWave}";
        _rewardAmountText.text = totalReward.ToString("N0");
        _totalRewardText.text = canDoubleReward
            ? $"Watch Ad to Get {(long)snapshot.BaseCurrencyReward * 2:N0}"
            : $"Total Earned: {totalReward:N0}";
        _doubleRewardButton.interactable = canDoubleReward;
        _doubleRewardButtonText.text = canDoubleReward
            ? "▶  Watch Ad for 2x Rewards"
            : "2x Rewards Claimed";
        _restartButton.interactable = true;
        _mainMenuButton.interactable = true;
    }

    public void SetBusy(bool isBusy)
    {
        if (!isBusy)
            return;

        _continueButton.interactable = false;
        _finishButton.interactable = false;
        _doubleRewardButton.interactable = false;
        _restartButton.interactable = false;
        _mainMenuButton.interactable = false;
    }

    public void Hide()
    {
        if (_root != null)
            _root.SetActive(false);
    }

    private void Build()
    {
        GameObject canvasObject = CreateObject("GameEndCanvas", transform);
        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080f, 1920f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;
        canvasObject.AddComponent<GraphicRaycaster>();

        _root = CreateObject("GameEndOverlay", canvasObject.transform);
        Stretch(_root.GetComponent<RectTransform>());
        Image overlay = _root.AddComponent<Image>();
        overlay.color = new Color(0f, 0f, 0f, 0.78f);

        _gameOverPanel = CreatePanel("GameOverPanel", _root.transform, new Vector2(860f, 710f));
        BuildGameOverPanel(_gameOverPanel.transform);

        _resultPanel = CreatePanel("GameResultPanel", _root.transform, new Vector2(860f, 850f));
        BuildResultPanel(_resultPanel.transform);
    }

    private void BuildGameOverPanel(Transform parent)
    {
        CreateText("Title", parent, "GAME OVER", 68f, TitleColor, 92f, FontStyles.Bold);
        CreateText("Subtitle", parent, "All Towers Destroyed", 30f, MutedTextColor, 58f);
        AddSpacer(parent, 16f);
        _gameOverStatsText = CreateText(
            "Stats",
            parent,
            string.Empty,
            29f,
            Color.white,
            70f,
            FontStyles.Bold);
        AddSpacer(parent, 18f);
        _continueButton = CreateButton(
            "ContinueButton",
            parent,
            "▶  Watch Ad to Continue",
            false,
            out _continueButtonText);
        _continueButton.onClick.AddListener(() => ContinueClicked?.Invoke());
        _finishButton = CreateButton(
            "FinishButton",
            parent,
            "End Run & Claim Rewards",
            true,
            out _);
        _finishButton.onClick.AddListener(() => FinishClicked?.Invoke());
    }

    private void BuildResultPanel(Transform parent)
    {
        CreateText("Title", parent, "GAME RESULT", 64f, TitleColor, 88f, FontStyles.Bold);
        _resultSummaryText = CreateText(
            "Summary",
            parent,
            string.Empty,
            30f,
            Color.white,
            58f,
            FontStyles.Bold);
        AddSpacer(parent, 10f);
        GameObject rewardBlock = CreateObject("RewardBlock", parent);
        LayoutElement rewardBlockLayout = rewardBlock.AddComponent<LayoutElement>();
        rewardBlockLayout.preferredHeight = 240f;
        Image rewardBlockImage = rewardBlock.AddComponent<Image>();
        rewardBlockImage.sprite = RoundedSpriteProvider.Sprite;
        rewardBlockImage.type = Image.Type.Sliced;
        rewardBlockImage.color = new Color(0.025f, 0.03f, 0.045f, 0.96f);
        VerticalLayoutGroup rewardBlockGroup = rewardBlock.AddComponent<VerticalLayoutGroup>();
        rewardBlockGroup.padding = new RectOffset(24, 24, 18, 18);
        rewardBlockGroup.spacing = 2f;
        rewardBlockGroup.childAlignment = TextAnchor.MiddleCenter;
        rewardBlockGroup.childControlHeight = true;
        rewardBlockGroup.childControlWidth = true;
        rewardBlockGroup.childForceExpandHeight = false;
        rewardBlockGroup.childForceExpandWidth = true;

        CreateText("RewardLabel", rewardBlock.transform, "Currency Earned", 26f, MutedTextColor, 40f);
        _rewardAmountText = CreateText(
            "RewardAmount",
            rewardBlock.transform,
            "0",
            82f,
            TitleColor,
            104f,
            FontStyles.Bold);
        _totalRewardText = CreateText(
            "TotalReward",
            rewardBlock.transform,
            string.Empty,
            27f,
            MutedTextColor,
            42f);
        AddSpacer(parent, 10f);
        _doubleRewardButton = CreateButton(
            "DoubleRewardButton",
            parent,
            "▶  Watch Ad for 2x Rewards",
            false,
            out _doubleRewardButtonText);
        _doubleRewardButton.onClick.AddListener(() => DoubleRewardClicked?.Invoke());
        AddSpacer(parent, 10f);

        GameObject row = CreateObject("NavigationButtons", parent);
        LayoutElement rowLayout = row.AddComponent<LayoutElement>();
        rowLayout.preferredHeight = 100f;
        HorizontalLayoutGroup rowGroup = row.AddComponent<HorizontalLayoutGroup>();
        rowGroup.spacing = 18f;
        rowGroup.childControlHeight = true;
        rowGroup.childControlWidth = true;
        rowGroup.childForceExpandHeight = true;
        rowGroup.childForceExpandWidth = true;

        _restartButton = CreateButton("RestartButton", row.transform, "Retry", true, out _);
        _restartButton.onClick.AddListener(() => RestartClicked?.Invoke());
        _mainMenuButton = CreateButton(
            "MainMenuButton",
            row.transform,
            "Main Menu",
            false,
            out TMP_Text mainMenuButtonText);
        _mainMenuButton.GetComponent<Image>().color = SecondaryColor;
        mainMenuButtonText.color = Color.white;
        _mainMenuButton.onClick.AddListener(() => MainMenuClicked?.Invoke());
    }

    private static GameObject CreatePanel(string name, Transform parent, Vector2 size)
    {
        GameObject panel = CreateObject(name, parent);
        RectTransform rect = panel.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = size;

        Image image = panel.AddComponent<Image>();
        image.sprite = RoundedSpriteProvider.Sprite;
        image.type = Image.Type.Sliced;
        image.color = PanelColor;

        VerticalLayoutGroup layout = panel.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(58, 58, 48, 48);
        layout.spacing = 16f;
        layout.childAlignment = TextAnchor.UpperCenter;
        layout.childControlHeight = true;
        layout.childControlWidth = true;
        layout.childForceExpandHeight = false;
        layout.childForceExpandWidth = true;
        return panel;
    }

    private static Button CreateButton(
        string name,
        Transform parent,
        string label,
        bool primary,
        out TMP_Text labelText)
    {
        GameObject buttonObject = CreateObject(name, parent);
        LayoutElement layout = buttonObject.AddComponent<LayoutElement>();
        layout.preferredHeight = 100f;
        layout.minHeight = 88f;

        Image image = buttonObject.AddComponent<Image>();
        image.sprite = RoundedSpriteProvider.Sprite;
        image.type = Image.Type.Sliced;
        image.color = primary ? PrimaryColor : new Color(0.08f, 0.09f, 0.11f, 0.18f);

        if (!primary)
        {
            Outline outline = buttonObject.AddComponent<Outline>();
            outline.effectColor = new Color(0.56f, 0.59f, 0.64f, 1f);
            outline.effectDistance = new Vector2(2f, -2f);
        }

        Button button = buttonObject.AddComponent<Button>();
        ColorBlock colors = button.colors;
        colors.highlightedColor = primary
            ? new Color(1f, 0.86f, 0.35f, 1f)
            : new Color(0.16f, 0.17f, 0.2f, 1f);
        colors.pressedColor = primary
            ? new Color(0.88f, 0.65f, 0.08f, 1f)
            : new Color(0.11f, 0.12f, 0.15f, 1f);
        colors.disabledColor = new Color(0.16f, 0.17f, 0.19f, 0.65f);
        button.colors = colors;

        labelText = CreateText(
            "Label",
            buttonObject.transform,
            label,
            29f,
            primary ? new Color(0.045f, 0.045f, 0.045f, 1f) : MutedTextColor,
            100f,
            FontStyles.Bold);
        Stretch(labelText.rectTransform);
        return button;
    }

    private static TMP_Text CreateText(
        string name,
        Transform parent,
        string value,
        float fontSize,
        Color color,
        float height,
        FontStyles fontStyle = FontStyles.Normal)
    {
        GameObject textObject = CreateObject(name, parent);
        LayoutElement layout = textObject.AddComponent<LayoutElement>();
        layout.preferredHeight = height;

        TextMeshProUGUI text = textObject.AddComponent<TextMeshProUGUI>();
        text.text = value;
        text.fontSize = fontSize;
        text.fontStyle = fontStyle;
        text.color = color;
        text.alignment = TextAlignmentOptions.Center;
        text.enableAutoSizing = true;
        text.fontSizeMin = Mathf.Max(16f, fontSize * 0.58f);
        text.fontSizeMax = fontSize;
        text.textWrappingMode = TextWrappingModes.NoWrap;
        text.raycastTarget = false;
        return text;
    }

    private static void AddSpacer(Transform parent, float height)
    {
        GameObject spacer = CreateObject("Spacer", parent);
        LayoutElement layout = spacer.AddComponent<LayoutElement>();
        layout.preferredHeight = height;
    }

    private static GameObject CreateObject(string name, Transform parent)
    {
        GameObject gameObject = new GameObject(name, typeof(RectTransform));
        gameObject.transform.SetParent(parent, false);
        return gameObject;
    }

    private static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    private static class RoundedSpriteProvider
    {
        private static Sprite _sprite;

        public static Sprite Sprite
        {
            get
            {
                if (_sprite == null)
                    _sprite = CreateRoundedSprite();

                return _sprite;
            }
        }

        private static Sprite CreateRoundedSprite()
        {
            const int size = 64;
            const float radius = 30f;
            Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            texture.name = "RuntimeRoundedRectangle";
            texture.wrapMode = TextureWrapMode.Clamp;
            Color32[] pixels = new Color32[size * size];

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = Mathf.Max(0f, Mathf.Abs(x - (size - 1) * 0.5f) - ((size - 1) * 0.5f - radius));
                    float dy = Mathf.Max(0f, Mathf.Abs(y - (size - 1) * 0.5f) - ((size - 1) * 0.5f - radius));
                    float distance = Mathf.Sqrt(dx * dx + dy * dy);
                    byte alpha = (byte)Mathf.RoundToInt(Mathf.Clamp01(radius + 0.5f - distance) * 255f);
                    pixels[y * size + x] = new Color32(255, 255, 255, alpha);
                }
            }

            texture.SetPixels32(pixels);
            texture.Apply(false, true);
            return Sprite.Create(
                texture,
                new Rect(0f, 0f, size, size),
                new Vector2(0.5f, 0.5f),
                100f,
                0,
                SpriteMeshType.FullRect,
                new Vector4(radius, radius, radius, radius));
        }
    }
}
