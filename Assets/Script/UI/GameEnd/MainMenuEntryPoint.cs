using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class MainMenuEntryPoint
{
    private const string RootName = "Core9MainMenu";
    private const string LegacySceneName = "SampleScene";

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void RegisterSceneCallback()
    {
        SceneManager.sceneLoaded -= HandleSceneLoaded;
        SceneManager.sceneLoaded += HandleSceneLoaded;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void BuildInitialScene()
    {
        TryBuild(SceneManager.GetActiveScene());
    }

    private static void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        TryBuild(scene);
    }

    private static void TryBuild(Scene scene)
    {
        if (scene.name != LegacySceneName || GameObject.Find(RootName) != null)
            return;

        GameObject root = new GameObject(RootName, typeof(RectTransform));
        Canvas canvas = root.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = root.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080f, 1920f);
        scaler.matchWidthOrHeight = 0.5f;
        root.AddComponent<GraphicRaycaster>();

        if (EventSystem.current == null)
        {
            GameObject eventSystemObject = new GameObject(
                "EventSystem",
                typeof(EventSystem),
                typeof(StandaloneInputModule));
            eventSystemObject.transform.SetParent(root.transform, false);
        }

        Image background = root.AddComponent<Image>();
        background.color = new Color(0.025f, 0.03f, 0.045f, 1f);

        GameObject content = CreateUiObject("Content", root.transform);
        RectTransform contentRect = content.GetComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0.5f, 0.5f);
        contentRect.anchorMax = new Vector2(0.5f, 0.5f);
        contentRect.sizeDelta = new Vector2(820f, 700f);

        VerticalLayoutGroup group = content.AddComponent<VerticalLayoutGroup>();
        group.spacing = 28f;
        group.childAlignment = TextAnchor.MiddleCenter;
        group.childControlHeight = true;
        group.childControlWidth = true;
        group.childForceExpandHeight = false;
        group.childForceExpandWidth = true;

        CreateText("Title", content.transform, "CORE9 DEFENSE", 72f, new Color(1f, 0.79f, 0.18f, 1f), 120f);
        CreateText("Subtitle", content.transform, "Ready for Battle", 30f, new Color(0.75f, 0.78f, 0.84f, 1f), 60f);

        var wallet = new PersistentCurrencyWallet();
        CreateText(
            "Currency",
            content.transform,
            $"Total Currency  {wallet.Balance:N0}",
            34f,
            Color.white,
            80f);

        GameObject buttonObject = CreateUiObject("StartBattleButton", content.transform);
        LayoutElement buttonLayout = buttonObject.AddComponent<LayoutElement>();
        buttonLayout.preferredHeight = 104f;
        Image buttonImage = buttonObject.AddComponent<Image>();
        buttonImage.color = new Color(1f, 0.78f, 0.12f, 1f);
        Button button = buttonObject.AddComponent<Button>();
        button.onClick.AddListener(() =>
            SceneManager.LoadScene(GameEndFlowManager.BattleSceneName, LoadSceneMode.Single));

        TMP_Text label = CreateText(
            "Label",
            buttonObject.transform,
            "Start Game",
            34f,
            new Color(0.04f, 0.04f, 0.04f, 1f),
            104f);
        RectTransform labelRect = label.rectTransform;
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = Vector2.zero;
        labelRect.offsetMax = Vector2.zero;
    }

    private static TMP_Text CreateText(
        string name,
        Transform parent,
        string value,
        float size,
        Color color,
        float height)
    {
        GameObject textObject = CreateUiObject(name, parent);
        LayoutElement layout = textObject.AddComponent<LayoutElement>();
        layout.preferredHeight = height;
        TextMeshProUGUI text = textObject.AddComponent<TextMeshProUGUI>();
        text.text = value;
        text.fontSize = size;
        text.fontStyle = FontStyles.Bold;
        text.color = color;
        text.alignment = TextAlignmentOptions.Center;
        text.enableAutoSizing = true;
        text.fontSizeMin = 18f;
        text.fontSizeMax = size;
        text.raycastTarget = false;
        return text;
    }

    private static GameObject CreateUiObject(string name, Transform parent)
    {
        GameObject gameObject = new GameObject(name, typeof(RectTransform));
        gameObject.transform.SetParent(parent, false);
        return gameObject;
    }
}
