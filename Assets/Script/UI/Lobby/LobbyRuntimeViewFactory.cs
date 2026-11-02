using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

public static class LobbyRuntimeViewFactory
{
    private static readonly Color Background = new Color(0.025f, 0.035f, 0.065f, 1f);
    private static readonly Color Panel = new Color(0.075f, 0.095f, 0.14f, 1f);
    private static readonly Color Card = new Color(0.095f, 0.12f, 0.17f, 1f);
    private static readonly Color Muted = new Color(0.62f, 0.67f, 0.76f, 1f);
    private static Sprite _roundedSprite;

    public static void Build(
        PersistentUpgradeCatalog catalog,
        out LobbyView lobbyView,
        out UpgradeView upgradeView)
    {
        if (EventSystem.current == null)
            new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));

        GameObject canvasObject = new GameObject(
            "LobbyCanvas",
            typeof(RectTransform),
            typeof(Canvas),
            typeof(CanvasScaler),
            typeof(GraphicRaycaster));
        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080f, 1920f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        GameObject background = UiObject("Background", canvasObject.transform);
        Stretch(background.GetComponent<RectTransform>());
        background.AddComponent<Image>().color = Background;
        GameObject safeArea = UiObject("SafeArea", canvasObject.transform);
        Stretch(safeArea.GetComponent<RectTransform>());
        safeArea.AddComponent<SafeAreaFitter>();

        lobbyView = BuildLobby(safeArea.transform);
        upgradeView = BuildUpgrade(safeArea.transform, catalog);
        upgradeView.gameObject.SetActive(false);
    }

    private static LobbyView BuildLobby(Transform parent)
    {
        GameObject root = UiObject("LobbyMain", parent);
        Stretch(root.GetComponent<RectTransform>());
        LobbyView view = root.AddComponent<LobbyView>();
        TMP_Text logo = Text("Logo", root.transform, "CORE9\nDEFENSE", 82f, Color.white, FontStyles.Bold);
        Anchors(logo.rectTransform, new Vector2(0.1f, 0.76f), new Vector2(0.9f, 0.93f));
        logo.characterSpacing = 8f;

        Button play = Button("PlayButton", root.transform, "PLAY", Color.white, Background, 54f, out _);
        Anchors(play.GetComponent<RectTransform>(), new Vector2(0.17f, 0.43f), new Vector2(0.83f, 0.53f));

        GameObject bottom = UiObject("BottomNavigation", root.transform);
        Anchors(bottom.GetComponent<RectTransform>(), new Vector2(0.07f, 0.055f), new Vector2(0.93f, 0.145f));
        HorizontalLayoutGroup layout = bottom.AddComponent<HorizontalLayoutGroup>();
        layout.spacing = 20f;
        layout.childControlHeight = true;
        layout.childControlWidth = true;
        layout.childForceExpandHeight = true;
        layout.childForceExpandWidth = true;
        Button upgrade = Button("UpgradeButton", bottom.transform, "Upgrade", Panel, Color.white, 28f, out _);
        Button rank = Button("RankButton", bottom.transform, "Rank", Panel, Color.white, 28f, out _);
        Button settings = Button("SettingsButton", bottom.transform, "Settings", Panel, Color.white, 28f, out _);
        view.Configure(root, play, upgrade, rank, settings);
        return view;
    }

    private static UpgradeView BuildUpgrade(Transform parent, PersistentUpgradeCatalog catalog)
    {
        GameObject root = UiObject("UpgradeScreen", parent);
        Stretch(root.GetComponent<RectTransform>());
        UpgradeView view = root.AddComponent<UpgradeView>();

        Button back = Button("BackButton", root.transform, "‹  BACK", Panel, Color.white, 26f, out _);
        Anchors(back.GetComponent<RectTransform>(), new Vector2(0.05f, 0.89f), new Vector2(0.27f, 0.95f));
        TMP_Text title = Text("Title", root.transform, "UPGRADE", 48f, Color.white, FontStyles.Bold);
        Anchors(title.rectTransform, new Vector2(0.28f, 0.89f), new Vector2(0.72f, 0.95f));

        GameObject currencyPill = UiObject("CurrencyPill", root.transform);
        Anchors(currencyPill.GetComponent<RectTransform>(), new Vector2(0.73f, 0.89f), new Vector2(0.95f, 0.95f));
        AddPanelImage(currencyPill, Panel);
        TMP_Text currency = Text("Currency", currencyPill.transform, "0", 27f, Color.white, FontStyles.Bold);
        Stretch(currency.rectTransform);

        GameObject tabs = UiObject("TargetTabs", root.transform);
        Anchors(tabs.GetComponent<RectTransform>(), new Vector2(0.12f, 0.80f), new Vector2(0.88f, 0.865f));
        HorizontalLayoutGroup tabLayout = tabs.AddComponent<HorizontalLayoutGroup>();
        tabLayout.spacing = 18f;
        tabLayout.childControlHeight = true;
        tabLayout.childControlWidth = true;
        tabLayout.childForceExpandHeight = true;
        tabLayout.childForceExpandWidth = true;
        Button coreTab = Button("CoreTab", tabs.transform, "CORE", Color.white, Color.black, 28f, out TMP_Text coreText);
        Button towerTab = Button("TowerTab", tabs.transform, "TOWER", Panel, Color.white, 28f, out TMP_Text towerText);

        GameObject selector = UiObject("TowerSelector", root.transform);
        Anchors(selector.GetComponent<RectTransform>(), new Vector2(0.045f, 0.665f), new Vector2(0.955f, 0.775f));
        HorizontalLayoutGroup towerLayout = selector.AddComponent<HorizontalLayoutGroup>();
        towerLayout.spacing = 10f;
        towerLayout.childControlHeight = true;
        towerLayout.childControlWidth = true;
        towerLayout.childForceExpandHeight = true;
        towerLayout.childForceExpandWidth = true;
        TowerSelectionItemView[] towerItems = catalog.TowerParts
            .Where(part => part != null)
            .OrderBy(part => (int)part.TowerType)
            .Select(part => TowerItem(selector.transform, part))
            .ToArray();

        TMP_Text target = Text("TargetName", root.transform, "ALL TOWERS", 27f, Muted, FontStyles.Bold);
        Anchors(target.rectTransform, new Vector2(0.08f, 0.61f), new Vector2(0.92f, 0.655f));

        GameObject list = UiObject("UpgradeList", root.transform);
        Anchors(list.GetComponent<RectTransform>(), new Vector2(0.045f, 0.04f), new Vector2(0.955f, 0.605f));
        VerticalLayoutGroup listLayout = list.AddComponent<VerticalLayoutGroup>();
        listLayout.spacing = 10f;
        listLayout.childControlHeight = true;
        listLayout.childControlWidth = true;
        listLayout.childForceExpandHeight = false;
        listLayout.childForceExpandWidth = true;
        SectionTitle(list.transform, "FLAT UPGRADES", "Direct stat increases");
        UpgradeRowView[] flatRows = Rows(list.transform, PermanentUpgradeKind.Flat);
        Divider(list.transform);
        SectionTitle(list.transform, "RATE UPGRADES", "Each stat unlocks at Flat Lv.10");
        UpgradeRowView[] rateRows = Rows(list.transform, PermanentUpgradeKind.Rate);

        view.Configure(
            root,
            back,
            currency,
            coreTab,
            coreTab.GetComponent<Image>(),
            coreText,
            towerTab,
            towerTab.GetComponent<Image>(),
            towerText,
            selector,
            target,
            towerItems,
            flatRows,
            rateRows);
        return view;
    }

    private static TowerSelectionItemView TowerItem(Transform parent, TowerPartData part)
    {
        GameObject item = UiObject($"{part.TowerType}TowerButton", parent);
        Image background = AddPanelImage(item, Panel);
        Button button = item.AddComponent<Button>();
        button.targetGraphic = background;
        GameObject iconObject = UiObject("Icon", item.transform);
        Anchors(iconObject.GetComponent<RectTransform>(), new Vector2(0.16f, 0.31f), new Vector2(0.84f, 0.9f));
        Image icon = iconObject.AddComponent<Image>();
        icon.sprite = part.Sprite;
        icon.preserveAspect = true;
        TMP_Text label = Text("Name", item.transform, part.TowerType.ToString(), 20f, Color.white, FontStyles.Bold);
        Anchors(label.rectTransform, new Vector2(0.04f, 0.04f), new Vector2(0.96f, 0.29f));
        TowerSelectionItemView view = item.AddComponent<TowerSelectionItemView>();
        view.Configure(part, button, background, icon, label);
        return view;
    }

    private static UpgradeRowView[] Rows(Transform parent, PermanentUpgradeKind kind)
    {
        TowerStatType[] stats =
        {
            TowerStatType.AttackDamage,
            TowerStatType.AttackSpeed,
            TowerStatType.MaxHp
        };
        return stats.Select(stat => Row(parent, stat, kind)).ToArray();
    }

    private static UpgradeRowView Row(
        Transform parent,
        TowerStatType stat,
        PermanentUpgradeKind kind)
    {
        GameObject row = UiObject($"{kind}{stat}Row", parent);
        row.AddComponent<LayoutElement>().preferredHeight = 104f;
        AddPanelImage(row, Card);
        HorizontalLayoutGroup layout = row.AddComponent<HorizontalLayoutGroup>();
        layout.padding = new RectOffset(22, 16, 12, 12);
        layout.spacing = 12f;
        layout.childAlignment = TextAnchor.MiddleLeft;
        layout.childControlHeight = true;
        layout.childControlWidth = true;
        layout.childForceExpandHeight = true;
        layout.childForceExpandWidth = false;
        TMP_Text name = Column("Stat", row.transform, "Attack Damage", 24f, Color.white, 230f, TextAlignmentOptions.Left);
        TMP_Text level = Column("Level", row.transform, "Lv.0", 22f, Muted, 92f, TextAlignmentOptions.Center);
        TMP_Text amount = Column("Amount", row.transform, "+0", 22f, Color.white, 125f, TextAlignmentOptions.Center);

        GameObject lockGroup = UiObject("LockRequirement", row.transform);
        lockGroup.AddComponent<LayoutElement>().preferredWidth = 170f;
        HorizontalLayoutGroup lockLayout = lockGroup.AddComponent<HorizontalLayoutGroup>();
        lockLayout.spacing = 7f;
        lockLayout.childAlignment = TextAnchor.MiddleCenter;
        lockLayout.childControlHeight = true;
        lockLayout.childControlWidth = false;
        TMP_Text lockGlyph = Column("LockIcon", lockGroup.transform, "■", 22f, Muted, 26f, TextAlignmentOptions.Center);
        lockGlyph.fontStyle = FontStyles.Bold;
        TMP_Text requirement = Column("Requirement", lockGroup.transform, "Flat Lv.10", 17f, Muted, 122f, TextAlignmentOptions.Left);
        Button purchase = Button("PurchaseButton", row.transform, "10", Color.white, Background, 22f, out TMP_Text purchaseText);
        LayoutElement purchaseLayout = purchase.gameObject.AddComponent<LayoutElement>();
        purchaseLayout.preferredWidth = 150f;
        purchaseLayout.preferredHeight = 76f;
        UpgradeRowView view = row.AddComponent<UpgradeRowView>();
        view.Configure(stat, kind, name, level, amount, purchase, purchaseText, lockGroup, requirement);
        return view;
    }

    private static void SectionTitle(Transform parent, string title, string subtitle)
    {
        GameObject container = UiObject(title.Replace(" ", string.Empty), parent);
        container.AddComponent<LayoutElement>().preferredHeight = 64f;
        TMP_Text titleText = Text("Title", container.transform, title, 27f, Color.white, FontStyles.Bold);
        Anchors(titleText.rectTransform, new Vector2(0f, 0.38f), new Vector2(0.55f, 1f));
        titleText.alignment = TextAlignmentOptions.Left;
        TMP_Text subtitleText = Text("Subtitle", container.transform, subtitle, 19f, Muted, FontStyles.Normal);
        Anchors(subtitleText.rectTransform, new Vector2(0.45f, 0.38f), new Vector2(1f, 1f));
        subtitleText.alignment = TextAlignmentOptions.Right;
    }

    private static void Divider(Transform parent)
    {
        GameObject divider = UiObject("Divider", parent);
        divider.AddComponent<LayoutElement>().preferredHeight = 2f;
        divider.AddComponent<Image>().color = new Color(0.3f, 0.34f, 0.42f, 0.7f);
    }

    private static TMP_Text Column(
        string name,
        Transform parent,
        string value,
        float fontSize,
        Color color,
        float width,
        TextAlignmentOptions alignment)
    {
        TMP_Text text = Text(name, parent, value, fontSize, color, FontStyles.Normal);
        text.gameObject.AddComponent<LayoutElement>().preferredWidth = width;
        text.alignment = alignment;
        return text;
    }

    private static Button Button(
        string name,
        Transform parent,
        string label,
        Color color,
        Color textColor,
        float fontSize,
        out TMP_Text text)
    {
        GameObject buttonObject = UiObject(name, parent);
        Image image = AddPanelImage(buttonObject, color);
        Button button = buttonObject.AddComponent<Button>();
        button.targetGraphic = image;
        ColorBlock colors = button.colors;
        colors.disabledColor = new Color(0.16f, 0.18f, 0.22f, 1f);
        button.colors = colors;
        text = Text("Label", buttonObject.transform, label, fontSize, textColor, FontStyles.Bold);
        Stretch(text.rectTransform);
        return button;
    }

    private static Image AddPanelImage(GameObject gameObject, Color color)
    {
        Image image = gameObject.AddComponent<Image>();
        image.sprite = RoundedSprite;
        image.type = Image.Type.Sliced;
        image.color = color;
        return image;
    }

    private static TMP_Text Text(
        string name,
        Transform parent,
        string value,
        float size,
        Color color,
        FontStyles style)
    {
        GameObject textObject = UiObject(name, parent);
        TextMeshProUGUI text = textObject.AddComponent<TextMeshProUGUI>();
        text.text = value;
        text.fontSize = size;
        text.fontStyle = style;
        text.color = color;
        text.alignment = TextAlignmentOptions.Center;
        text.enableAutoSizing = true;
        text.fontSizeMin = Mathf.Max(12f, size * 0.58f);
        text.fontSizeMax = size;
        text.raycastTarget = false;
        return text;
    }

    private static GameObject UiObject(string name, Transform parent)
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

    private static void Anchors(RectTransform rect, Vector2 min, Vector2 max)
    {
        rect.anchorMin = min;
        rect.anchorMax = max;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    private static Sprite RoundedSprite
    {
        get
        {
            if (_roundedSprite == null)
                _roundedSprite = CreateRoundedSprite();

            return _roundedSprite;
        }
    }

    private static Sprite CreateRoundedSprite()
    {
        const int size = 64;
        const float radius = 20f;
        var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        texture.name = "LobbyRoundedRectangle";
        texture.wrapMode = TextureWrapMode.Clamp;
        var pixels = new Color32[size * size];

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = Mathf.Max(0f, Mathf.Abs(x - 31.5f) - (31.5f - radius));
                float dy = Mathf.Max(0f, Mathf.Abs(y - 31.5f) - (31.5f - radius));
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
