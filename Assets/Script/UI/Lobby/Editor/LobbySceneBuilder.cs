#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class LobbySceneBuilder
{
    private const string ScenePath = "Assets/Scenes/Lobby.unity";
    private const string CatalogPath = "Assets/Data/PermanentUpgrades/PersistentUpgradeCatalog.asset";
    private const string RoundedSpritePath = "Assets/UI/Lobby/RoundedRectangle.png";
    private const string LockSpritePath = "Assets/UI/Lobby/LockIcon.png";

    private static readonly Color BackgroundColor = new Color(0.025f, 0.035f, 0.065f, 1f);
    private static readonly Color PanelColor = new Color(0.075f, 0.095f, 0.14f, 1f);
    private static readonly Color CardColor = new Color(0.095f, 0.12f, 0.17f, 1f);
    private static readonly Color MutedColor = new Color(0.62f, 0.67f, 0.76f, 1f);
    private static readonly Color WhiteButtonColor = new Color(0.96f, 0.97f, 1f, 1f);

    [MenuItem("Core9/Build Lobby Scene")]
    public static void Generate()
    {
        EnsureFolders();
        Sprite roundedSprite = CreateRoundedSprite();
        Sprite lockSprite = CreateLockSprite();
        PersistentUpgradeCatalog catalog = CreateOrLoadCatalog();
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        CreateCamera();
        CreateEventSystem();
        Canvas canvas = CreateCanvas();
        CreateBackground(canvas.transform);
        GameObject safeArea = CreateUiObject("SafeArea", canvas.transform);
        Stretch(safeArea.GetComponent<RectTransform>());
        safeArea.AddComponent<SafeAreaFitter>();

        GameObject controllerObject = new GameObject("LobbySceneController");
        LobbySceneController controller = controllerObject.AddComponent<LobbySceneController>();
        LobbyView lobbyView = BuildLobby(safeArea.transform, roundedSprite);
        UpgradeView upgradeView = BuildUpgrade(
            safeArea.transform,
            catalog,
            roundedSprite,
            lockSprite);
        controller.Configure(lobbyView, upgradeView, catalog);

        upgradeView.gameObject.SetActive(false);
        EditorSceneManager.SaveScene(scene, ScenePath);
        UpdateBuildSettings();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"Generated {ScenePath}");
    }

    private static void EnsureFolders()
    {
        Directory.CreateDirectory("Assets/UI/Lobby");
        Directory.CreateDirectory("Assets/Data/PermanentUpgrades");
    }

    private static PersistentUpgradeCatalog CreateOrLoadCatalog()
    {
        PersistentUpgradeCatalog catalog = AssetDatabase.LoadAssetAtPath<PersistentUpgradeCatalog>(CatalogPath);

        if (catalog != null)
            return catalog;

        catalog = ScriptableObject.CreateInstance<PersistentUpgradeCatalog>();
        var towerParts = AssetDatabase
            .FindAssets("t:TowerPartData", new[] { "Assets/Script/Tower/SO" })
            .Select(AssetDatabase.GUIDToAssetPath)
            .Select(AssetDatabase.LoadAssetAtPath<TowerPartData>)
            .Where(part => part != null)
            .GroupBy(part => part.TowerType)
            .Select(group => group.First())
            .OrderBy(part => (int)part.TowerType)
            .ToList();

        var definitions = new List<PersistentUpgradeDefinition>
        {
            CreateDefinition(TowerStatType.AttackDamage, "Attack Damage", 1f),
            CreateDefinition(TowerStatType.AttackSpeed, "Attack Speed", 0.05f),
            CreateDefinition(TowerStatType.MaxHp, "Max HP", 2f)
        };
        catalog.Configure(towerParts, definitions);
        AssetDatabase.CreateAsset(catalog, CatalogPath);
        EditorUtility.SetDirty(catalog);
        return catalog;
    }

    private static PersistentUpgradeDefinition CreateDefinition(
        TowerStatType statType,
        string displayName,
        float flatAmount)
    {
        var definition = new PersistentUpgradeDefinition();
        definition.Configure(
            statType,
            displayName,
            flatAmount,
            0.01f,
            10,
            200,
            10);
        return definition;
    }

    private static void CreateCamera()
    {
        GameObject cameraObject = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
        cameraObject.tag = "MainCamera";
        Camera camera = cameraObject.GetComponent<Camera>();
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = BackgroundColor;
        camera.orthographic = true;
        cameraObject.transform.position = new Vector3(0f, 0f, -10f);
    }

    private static void CreateEventSystem()
    {
        GameObject eventSystem = new GameObject("EventSystem", typeof(EventSystem));
        eventSystem.AddComponent<InputSystemUIInputModule>();
    }

    private static Canvas CreateCanvas()
    {
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
        return canvas;
    }

    private static void CreateBackground(Transform parent)
    {
        GameObject background = CreateUiObject("Background", parent);
        Stretch(background.GetComponent<RectTransform>());
        Image image = background.AddComponent<Image>();
        image.color = BackgroundColor;
        image.raycastTarget = false;
    }

    private static LobbyView BuildLobby(Transform parent, Sprite roundedSprite)
    {
        GameObject root = CreateUiObject("LobbyMain", parent);
        Stretch(root.GetComponent<RectTransform>());
        LobbyView view = root.AddComponent<LobbyView>();

        TMP_Text logo = CreateText("Logo", root.transform, "CORE9\nDEFENSE", 82f, Color.white, FontStyles.Bold);
        SetAnchors(logo.rectTransform, new Vector2(0.1f, 0.76f), new Vector2(0.9f, 0.93f));
        logo.characterSpacing = 8f;
        logo.lineSpacing = 10f;

        Button playButton = CreateButton(
            "PlayButton",
            root.transform,
            "PLAY",
            roundedSprite,
            WhiteButtonColor,
            new Color(0.03f, 0.05f, 0.09f, 1f),
            54f,
            out _);
        SetAnchors(
            playButton.GetComponent<RectTransform>(),
            new Vector2(0.17f, 0.43f),
            new Vector2(0.83f, 0.53f));

        GameObject bottomRow = CreateUiObject("BottomNavigation", root.transform);
        SetAnchors(
            bottomRow.GetComponent<RectTransform>(),
            new Vector2(0.07f, 0.055f),
            new Vector2(0.93f, 0.145f));
        HorizontalLayoutGroup bottomLayout = bottomRow.AddComponent<HorizontalLayoutGroup>();
        bottomLayout.spacing = 20f;
        bottomLayout.childAlignment = TextAnchor.MiddleCenter;
        bottomLayout.childControlHeight = true;
        bottomLayout.childControlWidth = true;
        bottomLayout.childForceExpandHeight = true;
        bottomLayout.childForceExpandWidth = true;

        Button upgradeButton = CreateButton(
            "UpgradeButton",
            bottomRow.transform,
            "Upgrade",
            roundedSprite,
            PanelColor,
            Color.white,
            28f,
            out _);
        Button rankButton = CreateButton(
            "RankButton",
            bottomRow.transform,
            "Rank",
            roundedSprite,
            PanelColor,
            Color.white,
            28f,
            out _);
        Button settingsButton = CreateButton(
            "SettingsButton",
            bottomRow.transform,
            "Settings",
            roundedSprite,
            PanelColor,
            Color.white,
            28f,
            out _);

        view.Configure(root, playButton, upgradeButton, rankButton, settingsButton);
        return view;
    }

    private static UpgradeView BuildUpgrade(
        Transform parent,
        PersistentUpgradeCatalog catalog,
        Sprite roundedSprite,
        Sprite lockSprite)
    {
        GameObject root = CreateUiObject("UpgradeScreen", parent);
        Stretch(root.GetComponent<RectTransform>());
        UpgradeView view = root.AddComponent<UpgradeView>();

        Button backButton = CreateButton(
            "BackButton",
            root.transform,
            "‹  BACK",
            roundedSprite,
            PanelColor,
            Color.white,
            26f,
            out _);
        SetAnchors(backButton.GetComponent<RectTransform>(), new Vector2(0.05f, 0.89f), new Vector2(0.27f, 0.95f));

        TMP_Text title = CreateText("Title", root.transform, "UPGRADE", 48f, Color.white, FontStyles.Bold);
        SetAnchors(title.rectTransform, new Vector2(0.28f, 0.89f), new Vector2(0.72f, 0.95f));

        GameObject currencyPill = CreateUiObject("CurrencyPill", root.transform);
        SetAnchors(currencyPill.GetComponent<RectTransform>(), new Vector2(0.73f, 0.89f), new Vector2(0.95f, 0.95f));
        Image currencyBackground = currencyPill.AddComponent<Image>();
        currencyBackground.sprite = roundedSprite;
        currencyBackground.type = Image.Type.Sliced;
        currencyBackground.color = PanelColor;
        TMP_Text currencyText = CreateText("Currency", currencyPill.transform, "0", 27f, Color.white, FontStyles.Bold);
        Stretch(currencyText.rectTransform);

        GameObject tabs = CreateUiObject("TargetTabs", root.transform);
        SetAnchors(tabs.GetComponent<RectTransform>(), new Vector2(0.12f, 0.80f), new Vector2(0.88f, 0.865f));
        HorizontalLayoutGroup tabLayout = tabs.AddComponent<HorizontalLayoutGroup>();
        tabLayout.spacing = 18f;
        tabLayout.childControlHeight = true;
        tabLayout.childControlWidth = true;
        tabLayout.childForceExpandHeight = true;
        tabLayout.childForceExpandWidth = true;
        Button coreTab = CreateButton(
            "CoreTab",
            tabs.transform,
            "CORE",
            roundedSprite,
            WhiteButtonColor,
            Color.black,
            28f,
            out TMP_Text coreTabText);
        Button towerTab = CreateButton(
            "TowerTab",
            tabs.transform,
            "TOWER",
            roundedSprite,
            PanelColor,
            Color.white,
            28f,
            out TMP_Text towerTabText);

        GameObject towerSelector = CreateUiObject("TowerSelector", root.transform);
        SetAnchors(towerSelector.GetComponent<RectTransform>(), new Vector2(0.045f, 0.665f), new Vector2(0.955f, 0.775f));
        HorizontalLayoutGroup towerLayout = towerSelector.AddComponent<HorizontalLayoutGroup>();
        towerLayout.spacing = 10f;
        towerLayout.childAlignment = TextAnchor.MiddleCenter;
        towerLayout.childControlHeight = true;
        towerLayout.childControlWidth = true;
        towerLayout.childForceExpandHeight = true;
        towerLayout.childForceExpandWidth = true;
        TowerSelectionItemView[] towerItems = catalog.TowerParts
            .Where(part => part != null)
            .OrderBy(part => (int)part.TowerType)
            .Select(part => CreateTowerItem(towerSelector.transform, part, roundedSprite))
            .ToArray();

        TMP_Text targetName = CreateText("TargetName", root.transform, "ALL TOWERS", 27f, MutedColor, FontStyles.Bold);
        SetAnchors(targetName.rectTransform, new Vector2(0.08f, 0.61f), new Vector2(0.92f, 0.655f));
        targetName.characterSpacing = 4f;

        GameObject list = CreateUiObject("UpgradeList", root.transform);
        SetAnchors(list.GetComponent<RectTransform>(), new Vector2(0.045f, 0.04f), new Vector2(0.955f, 0.605f));
        VerticalLayoutGroup listLayout = list.AddComponent<VerticalLayoutGroup>();
        listLayout.spacing = 10f;
        listLayout.padding = new RectOffset(0, 0, 0, 0);
        listLayout.childAlignment = TextAnchor.UpperCenter;
        listLayout.childControlHeight = true;
        listLayout.childControlWidth = true;
        listLayout.childForceExpandHeight = false;
        listLayout.childForceExpandWidth = true;

        CreateSectionTitle(list.transform, "FLAT UPGRADES", "Direct stat increases");
        UpgradeRowView[] flatRows = CreateRows(
            list.transform,
            PermanentUpgradeKind.Flat,
            roundedSprite,
            lockSprite);
        CreateDivider(list.transform);
        CreateSectionTitle(list.transform, "RATE UPGRADES", "Each stat unlocks at Flat Lv.10");
        UpgradeRowView[] rateRows = CreateRows(
            list.transform,
            PermanentUpgradeKind.Rate,
            roundedSprite,
            lockSprite);

        view.Configure(
            root,
            backButton,
            currencyText,
            coreTab,
            coreTab.GetComponent<Image>(),
            coreTabText,
            towerTab,
            towerTab.GetComponent<Image>(),
            towerTabText,
            towerSelector,
            targetName,
            towerItems,
            flatRows,
            rateRows);
        return view;
    }

    private static TowerSelectionItemView CreateTowerItem(
        Transform parent,
        TowerPartData towerPart,
        Sprite roundedSprite)
    {
        GameObject item = CreateUiObject($"{towerPart.TowerType}TowerButton", parent);
        Image background = item.AddComponent<Image>();
        background.sprite = roundedSprite;
        background.type = Image.Type.Sliced;
        background.color = PanelColor;
        Button button = item.AddComponent<Button>();
        button.targetGraphic = background;

        GameObject iconObject = CreateUiObject("Icon", item.transform);
        RectTransform iconRect = iconObject.GetComponent<RectTransform>();
        SetAnchors(iconRect, new Vector2(0.16f, 0.31f), new Vector2(0.84f, 0.9f));
        Image icon = iconObject.AddComponent<Image>();
        icon.sprite = towerPart.Sprite;
        icon.preserveAspect = true;
        icon.raycastTarget = false;

        TMP_Text label = CreateText(
            "Name",
            item.transform,
            towerPart.TowerType.ToString(),
            20f,
            Color.white,
            FontStyles.Bold);
        SetAnchors(label.rectTransform, new Vector2(0.04f, 0.04f), new Vector2(0.96f, 0.29f));

        TowerSelectionItemView view = item.AddComponent<TowerSelectionItemView>();
        view.Configure(towerPart, button, background, icon, label);
        return view;
    }

    private static void CreateSectionTitle(Transform parent, string title, string subtitle)
    {
        GameObject container = CreateUiObject(title.Replace(" ", string.Empty), parent);
        LayoutElement layout = container.AddComponent<LayoutElement>();
        layout.preferredHeight = 64f;
        TMP_Text titleText = CreateText("Title", container.transform, title, 27f, Color.white, FontStyles.Bold);
        SetAnchors(titleText.rectTransform, new Vector2(0f, 0.38f), new Vector2(0.55f, 1f));
        titleText.alignment = TextAlignmentOptions.Left;
        TMP_Text subtitleText = CreateText("Subtitle", container.transform, subtitle, 19f, MutedColor, FontStyles.Normal);
        SetAnchors(subtitleText.rectTransform, new Vector2(0.45f, 0.38f), new Vector2(1f, 1f));
        subtitleText.alignment = TextAlignmentOptions.Right;
    }

    private static UpgradeRowView[] CreateRows(
        Transform parent,
        PermanentUpgradeKind kind,
        Sprite roundedSprite,
        Sprite lockSprite)
    {
        TowerStatType[] stats =
        {
            TowerStatType.AttackDamage,
            TowerStatType.AttackSpeed,
            TowerStatType.MaxHp
        };
        var rows = new UpgradeRowView[stats.Length];

        for (int i = 0; i < stats.Length; i++)
            rows[i] = CreateRow(parent, stats[i], kind, roundedSprite, lockSprite);

        return rows;
    }

    private static UpgradeRowView CreateRow(
        Transform parent,
        TowerStatType statType,
        PermanentUpgradeKind kind,
        Sprite roundedSprite,
        Sprite lockSprite)
    {
        GameObject row = CreateUiObject($"{kind}{statType}Row", parent);
        LayoutElement rowLayout = row.AddComponent<LayoutElement>();
        rowLayout.preferredHeight = 104f;
        Image background = row.AddComponent<Image>();
        background.sprite = roundedSprite;
        background.type = Image.Type.Sliced;
        background.color = CardColor;
        HorizontalLayoutGroup layout = row.AddComponent<HorizontalLayoutGroup>();
        layout.padding = new RectOffset(22, 16, 12, 12);
        layout.spacing = 12f;
        layout.childAlignment = TextAnchor.MiddleLeft;
        layout.childControlHeight = true;
        layout.childControlWidth = true;
        layout.childForceExpandHeight = true;
        layout.childForceExpandWidth = false;

        TMP_Text statName = CreateColumnText("Stat", row.transform, "Attack Damage", 24f, Color.white, 230f, TextAlignmentOptions.Left);
        TMP_Text level = CreateColumnText("Level", row.transform, "Lv.0", 22f, MutedColor, 92f, TextAlignmentOptions.Center);
        TMP_Text amount = CreateColumnText("Amount", row.transform, "+0", 22f, Color.white, 125f, TextAlignmentOptions.Center);

        GameObject lockGroup = CreateUiObject("LockRequirement", row.transform);
        LayoutElement lockLayout = lockGroup.AddComponent<LayoutElement>();
        lockLayout.preferredWidth = 170f;
        HorizontalLayoutGroup lockGroupLayout = lockGroup.AddComponent<HorizontalLayoutGroup>();
        lockGroupLayout.spacing = 7f;
        lockGroupLayout.childAlignment = TextAnchor.MiddleCenter;
        lockGroupLayout.childControlHeight = true;
        lockGroupLayout.childControlWidth = false;
        lockGroupLayout.childForceExpandHeight = false;
        lockGroupLayout.childForceExpandWidth = false;
        GameObject lockImageObject = CreateUiObject("LockIcon", lockGroup.transform);
        LayoutElement lockImageLayout = lockImageObject.AddComponent<LayoutElement>();
        lockImageLayout.preferredWidth = 26f;
        lockImageLayout.preferredHeight = 32f;
        Image lockImage = lockImageObject.AddComponent<Image>();
        lockImage.sprite = lockSprite;
        lockImage.preserveAspect = true;
        lockImage.color = MutedColor;
        TMP_Text unlockText = CreateColumnText("Requirement", lockGroup.transform, "Flat Lv.10", 17f, MutedColor, 122f, TextAlignmentOptions.Left);

        Button purchaseButton = CreateButton(
            "PurchaseButton",
            row.transform,
            "10",
            roundedSprite,
            WhiteButtonColor,
            new Color(0.03f, 0.05f, 0.09f, 1f),
            22f,
            out TMP_Text purchaseText);
        LayoutElement purchaseLayout = purchaseButton.gameObject.AddComponent<LayoutElement>();
        purchaseLayout.preferredWidth = 150f;
        purchaseLayout.preferredHeight = 76f;

        UpgradeRowView view = row.AddComponent<UpgradeRowView>();
        view.Configure(
            statType,
            kind,
            statName,
            level,
            amount,
            purchaseButton,
            purchaseText,
            lockGroup,
            unlockText);
        return view;
    }

    private static TMP_Text CreateColumnText(
        string name,
        Transform parent,
        string value,
        float fontSize,
        Color color,
        float width,
        TextAlignmentOptions alignment)
    {
        TMP_Text text = CreateText(name, parent, value, fontSize, color, FontStyles.Normal);
        LayoutElement layout = text.gameObject.AddComponent<LayoutElement>();
        layout.preferredWidth = width;
        layout.flexibleWidth = 0f;
        text.alignment = alignment;
        return text;
    }

    private static void CreateDivider(Transform parent)
    {
        GameObject divider = CreateUiObject("Divider", parent);
        LayoutElement layout = divider.AddComponent<LayoutElement>();
        layout.preferredHeight = 2f;
        Image image = divider.AddComponent<Image>();
        image.color = new Color(0.3f, 0.34f, 0.42f, 0.7f);
        image.raycastTarget = false;
    }

    private static Button CreateButton(
        string name,
        Transform parent,
        string label,
        Sprite roundedSprite,
        Color backgroundColor,
        Color textColor,
        float fontSize,
        out TMP_Text labelText)
    {
        GameObject buttonObject = CreateUiObject(name, parent);
        Image image = buttonObject.AddComponent<Image>();
        image.sprite = roundedSprite;
        image.type = Image.Type.Sliced;
        image.color = backgroundColor;
        Button button = buttonObject.AddComponent<Button>();
        button.targetGraphic = image;
        ColorBlock colors = button.colors;
        colors.highlightedColor = new Color(0.86f, 0.89f, 0.96f, 1f);
        colors.pressedColor = new Color(0.7f, 0.74f, 0.82f, 1f);
        colors.disabledColor = new Color(0.16f, 0.18f, 0.22f, 1f);
        button.colors = colors;

        labelText = CreateText("Label", buttonObject.transform, label, fontSize, textColor, FontStyles.Bold);
        Stretch(labelText.rectTransform);
        return button;
    }

    private static TMP_Text CreateText(
        string name,
        Transform parent,
        string value,
        float fontSize,
        Color color,
        FontStyles style)
    {
        GameObject textObject = CreateUiObject(name, parent);
        TextMeshProUGUI text = textObject.AddComponent<TextMeshProUGUI>();
        text.text = value;
        text.fontSize = fontSize;
        text.fontStyle = style;
        text.color = color;
        text.alignment = TextAlignmentOptions.Center;
        text.enableAutoSizing = true;
        text.fontSizeMin = Mathf.Max(12f, fontSize * 0.58f);
        text.fontSizeMax = fontSize;
        text.raycastTarget = false;
        return text;
    }

    private static GameObject CreateUiObject(string name, Transform parent)
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

    private static void SetAnchors(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax)
    {
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    private static Sprite CreateRoundedSprite()
    {
        if (!File.Exists(RoundedSpritePath))
        {
            const int size = 64;
            const float radius = 18f;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            var pixels = new Color32[size * size];

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = Mathf.Max(radius - x - 0.5f, 0f, x + 0.5f - (size - radius));
                    float dy = Mathf.Max(radius - y - 0.5f, 0f, y + 0.5f - (size - radius));
                    float distance = Mathf.Sqrt(dx * dx + dy * dy);
                    byte alpha = (byte)Mathf.RoundToInt(255f * Mathf.Clamp01(radius + 0.5f - distance));
                    pixels[y * size + x] = new Color32(255, 255, 255, alpha);
                }
            }

            texture.SetPixels32(pixels);
            texture.Apply();
            File.WriteAllBytes(RoundedSpritePath, texture.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(texture);
        }

        ConfigureSpriteImporter(RoundedSpritePath, new Vector4(18f, 18f, 18f, 18f));
        return AssetDatabase.LoadAssetAtPath<Sprite>(RoundedSpritePath);
    }

    private static Sprite CreateLockSprite()
    {
        if (!File.Exists(LockSpritePath))
        {
            const int size = 32;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            var pixels = new Color32[size * size];

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    bool body = x >= 5 && x <= 26 && y >= 2 && y <= 17;
                    float dx = x - 15.5f;
                    float dy = y - 19f;
                    float distance = Mathf.Sqrt(dx * dx + dy * dy);
                    bool shackle = y >= 16 && distance >= 7f && distance <= 10f;
                    pixels[y * size + x] = body || shackle
                        ? new Color32(255, 255, 255, 255)
                        : new Color32(255, 255, 255, 0);
                }
            }

            texture.SetPixels32(pixels);
            texture.Apply();
            File.WriteAllBytes(LockSpritePath, texture.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(texture);
        }

        ConfigureSpriteImporter(LockSpritePath, Vector4.zero);
        return AssetDatabase.LoadAssetAtPath<Sprite>(LockSpritePath);
    }

    private static void ConfigureSpriteImporter(string path, Vector4 border)
    {
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
        var importer = (TextureImporter)AssetImporter.GetAtPath(path);
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.alphaIsTransparency = true;
        importer.mipmapEnabled = false;
        importer.spriteBorder = border;
        importer.SaveAndReimport();
    }

    private static void UpdateBuildSettings()
    {
        var scenes = new List<EditorBuildSettingsScene>
        {
            new EditorBuildSettingsScene(ScenePath, true)
        };
        const string battlePath = "Assets/Scenes/Battle.unity";

        if (File.Exists(battlePath))
            scenes.Add(new EditorBuildSettingsScene(battlePath, true));

        foreach (EditorBuildSettingsScene existing in EditorBuildSettings.scenes)
        {
            if (existing.path == ScenePath ||
                existing.path == battlePath ||
                existing.path == "Assets/Scenes/SampleScene.unity")
            {
                continue;
            }

            scenes.Add(existing);
        }

        EditorBuildSettings.scenes = scenes.ToArray();
    }
}
#endif
