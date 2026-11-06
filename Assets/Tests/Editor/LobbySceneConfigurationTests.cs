using System.Collections.Generic;
using NUnit.Framework;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public sealed class LobbySceneConfigurationTests
{
    [Test]
    public void LobbyScene_SerializedUiAndControllerReferencesAreComplete()
    {
        Scene scene = EditorSceneManager.OpenScene(
            "Assets/Scenes/Lobby.unity",
            OpenSceneMode.Single);

        Assert.That(FindObjects<LobbySceneController>(scene), Has.Count.EqualTo(1));
        Assert.That(FindObjects<LobbyView>(scene), Has.Count.EqualTo(1));
        Assert.That(FindObjects<UpgradeView>(scene), Has.Count.EqualTo(1));
        Assert.That(FindObjects<LobbyOptionsView>(scene), Has.Count.EqualTo(1));
        Assert.That(FindObjects<ExitConfirmationView>(scene), Has.Count.EqualTo(1));
        Assert.That(FindObjects<LobbyBackInput>(scene), Has.Count.EqualTo(1));
        Assert.That(FindObjects<Canvas>(scene), Has.Count.EqualTo(2));
        Assert.That(FindObjects<EventSystem>(scene), Has.Count.EqualTo(1));
        Assert.That(FindGameObject(scene, "LobbyCanvas"), Is.Not.Null);
        Assert.That(FindGameObject(scene, "LobbyMain"), Is.Not.Null);
        Assert.That(FindGameObject(scene, "UpgradeScreen"), Is.Not.Null);
        Assert.That(FindGameObject(scene, "OptionsPopupRoot"), Is.Not.Null);
        Assert.That(FindGameObject(scene, "OptionsPanel"), Is.Not.Null);
        Assert.That(FindGameObject(scene, "SceneTransition"), Is.Not.Null);
        Assert.That(FindGameObject(scene, "FadeCanvas"), Is.Not.Null);
        Assert.That(FindGameObject(scene, "FadeImage"), Is.Not.Null);
        Assert.That(FindGameObject(scene, "ExitPopupRoot"), Is.Not.Null);
        Assert.That(FindGameObject(scene, "ExitPopupPanel"), Is.Not.Null);

        LobbySceneController controller = FindObjects<LobbySceneController>(scene)[0];
        LobbyView lobbyView = FindObjects<LobbyView>(scene)[0];
        UpgradeView upgradeView = FindObjects<UpgradeView>(scene)[0];
        LobbyOptionsView optionsView = FindObjects<LobbyOptionsView>(scene)[0];
        SceneTransition sceneTransition = FindObjects<SceneTransition>(scene)[0];
        ExitConfirmationView exitView = FindObjects<ExitConfirmationView>(scene)[0];
        LobbyBackInput backInput = FindObjects<LobbyBackInput>(scene)[0];
        var serializedController = new SerializedObject(controller);

        Assert.That(
            serializedController.FindProperty("_lobbyView").objectReferenceValue,
            Is.SameAs(lobbyView));
        Assert.That(
            serializedController.FindProperty("_upgradeView").objectReferenceValue,
            Is.SameAs(upgradeView));
        Assert.That(
            serializedController.FindProperty("_upgradeCatalog").objectReferenceValue,
            Is.Not.Null);
        Assert.That(
            serializedController.FindProperty("_sceneTransition").objectReferenceValue,
            Is.SameAs(sceneTransition));
        Assert.That(
            serializedController.FindProperty("_optionsView").objectReferenceValue,
            Is.SameAs(optionsView));
        Assert.That(
            serializedController.FindProperty("_exitConfirmationView").objectReferenceValue,
            Is.SameAs(exitView));
        Assert.That(
            serializedController.FindProperty("_backInput").objectReferenceValue,
            Is.SameAs(backInput));

        Assert.That(exitView.IsConfigured, Is.True);
        AssertReferencesAssigned(exitView, "_root", "_cancelButton", "_exitButton");
        Assert.That(optionsView.IsConfigured, Is.True);
        AssertReferencesAssigned(
            optionsView,
            "_root",
            "_bgmSlider",
            "_bgmValueText",
            "_sfxSlider",
            "_sfxValueText",
            "_languageValueText",
            "_versionValueText",
            "_closeButton");

        GameObject optionsRoot = FindGameObject(scene, "OptionsPopupRoot");
        GameObject optionsDim = FindGameObject(scene, "OptionsDimBackground");
        GameObject optionsPanel = FindGameObject(scene, "OptionsPanel");
        var serializedOptionsView = new SerializedObject(optionsView);
        GameObject closeButton = ((Button)serializedOptionsView
            .FindProperty("_closeButton").objectReferenceValue).gameObject;
        Slider bgmSlider = (Slider)serializedOptionsView
            .FindProperty("_bgmSlider").objectReferenceValue;
        Slider sfxSlider = (Slider)serializedOptionsView
            .FindProperty("_sfxSlider").objectReferenceValue;
        Assert.That(optionsRoot.activeSelf, Is.False);
        Assert.That(optionsRoot.transform.parent.name, Is.EqualTo("LobbyCanvas"));
        Assert.That(optionsDim.GetComponent<Image>().raycastTarget, Is.True);
        Assert.That(optionsDim.transform.GetSiblingIndex(), Is.LessThan(optionsPanel.transform.GetSiblingIndex()));
        Assert.That(closeButton.transform.parent, Is.SameAs(optionsPanel.transform));
        Assert.That(bgmSlider.transform.parent, Is.SameAs(optionsPanel.transform));
        Assert.That(sfxSlider.transform.parent, Is.SameAs(optionsPanel.transform));
        Assert.That(optionsPanel.GetComponent<RectTransform>().rect.width, Is.LessThanOrEqualTo(860f));
        Assert.That(optionsPanel.GetComponent<RectTransform>().rect.height, Is.LessThanOrEqualTo(1240f));
        Assert.That(closeButton.GetComponent<RectTransform>().rect.height, Is.GreaterThanOrEqualTo(100f));
        Assert.That(bgmSlider.GetComponent<RectTransform>().rect.height, Is.GreaterThanOrEqualTo(76f));
        Assert.That(sfxSlider.GetComponent<RectTransform>().rect.height, Is.GreaterThanOrEqualTo(76f));
        Assert.That(bgmSlider.GetComponent<Image>().raycastTarget, Is.True);
        Assert.That(sfxSlider.GetComponent<Image>().raycastTarget, Is.True);
        Assert.That(bgmSlider.transform.Find("Background").GetComponent<RectTransform>().rect.height, Is.EqualTo(20f));
        Assert.That(sfxSlider.transform.Find("Background").GetComponent<RectTransform>().rect.height, Is.EqualTo(20f));
        Assert.That(bgmSlider.transform.Find("FillArea").GetComponent<RectTransform>().rect.height, Is.EqualTo(30f));
        Assert.That(sfxSlider.transform.Find("FillArea").GetComponent<RectTransform>().rect.height, Is.EqualTo(30f));
        Assert.That(bgmSlider.handleRect.rect.size, Is.EqualTo(new Vector2(50f, 50f)));
        Assert.That(sfxSlider.handleRect.rect.size, Is.EqualTo(new Vector2(50f, 50f)));
        Assert.That(bgmSlider.minValue, Is.Zero);
        Assert.That(bgmSlider.maxValue, Is.EqualTo(1f));
        Assert.That(bgmSlider.handleRect, Is.Not.Null);
        Assert.That(sfxSlider.handleRect, Is.Not.Null);
        Assert.That(optionsPanel.transform.Find("TitleText").GetComponent<TMP_Text>().text, Is.EqualTo("OPTIONS"));
        Transform audioHeader = optionsPanel.transform.Find("AudioHeader");
        Assert.That(
            audioHeader,
            Is.Not.Null,
            $"OptionsPanel children: {string.Join(", ", GetDirectChildNames(optionsPanel.transform))}");
        Assert.That(audioHeader.GetComponent<TMP_Text>().text, Is.EqualTo("AUDIO"));
        Assert.That(optionsPanel.transform.Find("LanguageHeader").GetComponent<TMP_Text>().text, Is.EqualTo("LANGUAGE"));
        Assert.That(optionsPanel.transform.Find("LanguageValueText").GetComponent<TMP_Text>().text, Is.EqualTo("English"));
        Assert.That(optionsPanel.transform.Find("InfoHeader").GetComponent<TMP_Text>().text, Is.EqualTo("INFO"));
        Assert.That(closeButton.transform.Find("Label").GetComponent<TMP_Text>().text, Is.EqualTo("CLOSE"));
        Assert.That(
            FindGameObject(scene, "SettingsButton").transform.Find("Label").GetComponent<TMP_Text>().text,
            Is.EqualTo("OPTIONS"));

        GameObject popupRoot = FindGameObject(scene, "ExitPopupRoot");
        GameObject dimBackground = FindGameObject(scene, "DimBackground");
        GameObject popupPanel = FindGameObject(scene, "ExitPopupPanel");
        GameObject cancelButton = FindGameObject(scene, "CancelButton");
        GameObject exitButton = FindGameObject(scene, "ExitButton");
        Assert.That(popupRoot.activeSelf, Is.False);
        Assert.That(popupRoot.transform.parent.name, Is.EqualTo("LobbyCanvas"));
        Assert.That(popupRoot.transform.GetSiblingIndex(), Is.EqualTo(popupRoot.transform.parent.childCount - 1));
        Assert.That(dimBackground.transform.GetSiblingIndex(), Is.LessThan(popupPanel.transform.GetSiblingIndex()));
        Assert.That(dimBackground.GetComponent<Image>().raycastTarget, Is.True);

        RectTransform popupRect = popupRoot.GetComponent<RectTransform>();
        RectTransform panelRect = popupPanel.GetComponent<RectTransform>();
        Assert.That(popupRect.anchorMin, Is.EqualTo(Vector2.zero));
        Assert.That(popupRect.anchorMax, Is.EqualTo(Vector2.one));
        Assert.That(panelRect.anchorMin, Is.EqualTo(new Vector2(0.5f, 0.5f)));
        Assert.That(panelRect.anchorMax, Is.EqualTo(new Vector2(0.5f, 0.5f)));
        Assert.That(panelRect.pivot, Is.EqualTo(new Vector2(0.5f, 0.5f)));
        Assert.That(cancelButton.GetComponent<RectTransform>().rect.height, Is.GreaterThanOrEqualTo(100f));
        Assert.That(exitButton.GetComponent<RectTransform>().rect.height, Is.GreaterThanOrEqualTo(100f));
        Assert.That(popupPanel.transform.Find("TitleText").GetComponent<TMP_Text>().text, Is.EqualTo("EXIT GAME?"));
        Assert.That(
            popupPanel.transform.Find("DescriptionText").GetComponent<TMP_Text>().text,
            Is.EqualTo("Are you sure you want to exit?"));
        Assert.That(cancelButton.transform.Find("Label").GetComponent<TMP_Text>().text, Is.EqualTo("CANCEL"));
        Assert.That(exitButton.transform.Find("Label").GetComponent<TMP_Text>().text, Is.EqualTo("EXIT"));

        var serializedTransition = new SerializedObject(sceneTransition);
        Assert.That(serializedTransition.FindProperty("_fadeOutDuration").floatValue, Is.EqualTo(0.35f));
        Assert.That(serializedTransition.FindProperty("_fadeInDuration").floatValue, Is.EqualTo(0.5f));
        Assert.That(serializedTransition.FindProperty("_fadeColor").colorValue, Is.EqualTo(Color.black));
        Assert.That(serializedTransition.FindProperty("_canvasGroup").objectReferenceValue, Is.Not.Null);
        Assert.That(serializedTransition.FindProperty("_fadeImage").objectReferenceValue, Is.Not.Null);

        Canvas fadeCanvas = FindGameObject(scene, "FadeCanvas").GetComponent<Canvas>();
        RectTransform fadeImage = FindGameObject(scene, "FadeImage").GetComponent<RectTransform>();
        Assert.That(fadeCanvas.renderMode, Is.EqualTo(RenderMode.ScreenSpaceOverlay));
        Assert.That(fadeCanvas.sortingOrder, Is.EqualTo(32767));
        Assert.That(fadeImage.anchorMin, Is.EqualTo(Vector2.zero));
        Assert.That(fadeImage.anchorMax, Is.EqualTo(Vector2.one));
        Assert.That(fadeImage.offsetMin, Is.EqualTo(Vector2.zero));
        Assert.That(fadeImage.offsetMax, Is.EqualTo(Vector2.zero));

        AssertReferencesAssigned(
            lobbyView,
            "_root",
            "_playButton",
            "_upgradeButton",
            "_rankButton",
            "_settingsButton");
        AssertReferencesAssigned(
            upgradeView,
            "_root",
            "_backButton",
            "_currencyText",
            "_coreTabButton",
            "_coreTabImage",
            "_coreTabText",
            "_towerTabButton",
            "_towerTabImage",
            "_towerTabText",
            "_towerSelectorRoot",
            "_targetNameText");
        AssertArrayAssigned(upgradeView, "_towerItems", 6);
        AssertArrayAssigned(upgradeView, "_flatRows", 3);
        AssertArrayAssigned(upgradeView, "_rateRows", 3);

        foreach (TowerSelectionItemView item in FindObjects<TowerSelectionItemView>(scene))
        {
            AssertReferencesAssigned(
                item,
                "_towerPart",
                "_button",
                "_background",
                "_towerImage",
                "_nameText");
        }

        foreach (UpgradeRowView row in FindObjects<UpgradeRowView>(scene))
        {
            AssertReferencesAssigned(
                row,
                "_statNameText",
                "_levelText",
                "_amountText",
                "_purchaseButton",
                "_purchaseButtonText",
                "_lockGroup",
                "_unlockRequirementText");
        }

        foreach (GameObject gameObject in GetAllGameObjects(scene))
        {
            Assert.That(
                GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(gameObject),
                Is.Zero,
                $"Missing Script on {GetHierarchyPath(gameObject.transform)}");
        }
    }

    [Test]
    public void UserAudioSettings_SaveAndRestoreSharedVolumes()
    {
        string keySuffix = System.Guid.NewGuid().ToString("N");
        string bgmKey = $"Core9.Tests.Bgm.{keySuffix}";
        string sfxKey = $"Core9.Tests.Sfx.{keySuffix}";

        try
        {
            var settings = new UserAudioSettings(bgmKey, sfxKey);
            Assert.That(settings.BgmVolume, Is.EqualTo(1f));
            Assert.That(settings.SfxVolume, Is.EqualTo(1f));

            settings.SetBgmVolume(0.42f);
            settings.SetSfxVolume(2f);

            var restoredSettings = new UserAudioSettings(bgmKey, sfxKey);
            Assert.That(restoredSettings.BgmVolume, Is.EqualTo(0.42f).Within(0.001f));
            Assert.That(restoredSettings.SfxVolume, Is.EqualTo(1f));
        }
        finally
        {
            PlayerPrefs.DeleteKey(bgmKey);
            PlayerPrefs.DeleteKey(sfxKey);
            PlayerPrefs.Save();
        }
    }

    [Test]
    public void OptionsPresenter_CloseAndBack_HideOptionsWithoutOpeningExitPopup()
    {
        Scene scene = EditorSceneManager.OpenScene(
            "Assets/Scenes/Lobby.unity",
            OpenSceneMode.Single);
        LobbyOptionsView optionsView = FindObjects<LobbyOptionsView>(scene)[0];
        ExitConfirmationView exitView = FindObjects<ExitConfirmationView>(scene)[0];
        LobbyBackInput backInput = FindObjects<LobbyBackInput>(scene)[0];
        SceneTransition sceneTransition = FindObjects<SceneTransition>(scene)[0];
        string keySuffix = System.Guid.NewGuid().ToString("N");
        string bgmKey = $"Core9.Tests.Bgm.{keySuffix}";
        string sfxKey = $"Core9.Tests.Sfx.{keySuffix}";
        var optionsPresenter = new LobbyOptionsPresenter(
            optionsView,
            new UserAudioSettings(bgmKey, sfxKey));
        var exitPresenter = new LobbyExitPresenter(
            exitView,
            backInput,
            sceneTransition,
            optionsPresenter.TryClose);

        try
        {
            optionsPresenter.Show();
            Assert.That(optionsView.IsVisible, Is.True);
            Assert.That(exitView.IsVisible, Is.False);

            var serializedOptionsView = new SerializedObject(optionsView);
            Slider bgmSlider = (Slider)serializedOptionsView
                .FindProperty("_bgmSlider").objectReferenceValue;
            Slider sfxSlider = (Slider)serializedOptionsView
                .FindProperty("_sfxSlider").objectReferenceValue;
            TMP_Text bgmValueText = (TMP_Text)serializedOptionsView
                .FindProperty("_bgmValueText").objectReferenceValue;
            TMP_Text sfxValueText = (TMP_Text)serializedOptionsView
                .FindProperty("_sfxValueText").objectReferenceValue;
            TMP_Text versionValueText = (TMP_Text)serializedOptionsView
                .FindProperty("_versionValueText").objectReferenceValue;

            Canvas.ForceUpdateCanvases();
            EventSystem eventSystem = FindObjects<EventSystem>(scene)[0];
            RectTransform bgmHandleArea = (RectTransform)bgmSlider.handleRect.parent;
            RectTransform sfxHandleArea = (RectTransform)sfxSlider.handleRect.parent;
            var pointer = new PointerEventData(eventSystem)
            {
                button = PointerEventData.InputButton.Left,
                position = GetScreenPosition(bgmHandleArea, 0f)
            };
            bgmSlider.OnPointerDown(pointer);
            Assert.That(bgmSlider.value, Is.EqualTo(0f).Within(0.01f));
            Assert.That(bgmValueText.text, Is.EqualTo("0%"));

            pointer.position = GetScreenPosition(bgmHandleArea, 0.5f);
            bgmSlider.OnDrag(pointer);
            Assert.That(bgmSlider.value, Is.EqualTo(0.5f).Within(0.01f));
            Assert.That(bgmValueText.text, Is.EqualTo("50%"));

            pointer.position = GetScreenPosition(bgmHandleArea, 1f);
            bgmSlider.OnDrag(pointer);
            Assert.That(bgmSlider.value, Is.EqualTo(1f).Within(0.01f));
            Assert.That(bgmValueText.text, Is.EqualTo("100%"));

            pointer.position = GetScreenPosition(sfxHandleArea, 0.5f);
            sfxSlider.OnPointerDown(pointer);
            Assert.That(sfxSlider.value, Is.EqualTo(0.5f).Within(0.01f));
            Assert.That(sfxValueText.text, Is.EqualTo("50%"));

            GraphicRaycaster raycaster = bgmSlider.GetComponentInParent<GraphicRaycaster>();
            pointer.position = GetScreenPosition(bgmSlider.GetComponent<RectTransform>(), 0.75f);
            var raycastResults = new List<RaycastResult>();
            raycaster.Raycast(pointer, raycastResults);
            Assert.That(
                raycastResults.Exists(result => result.gameObject == bgmSlider.gameObject),
                Is.True,
                "The transparent slider-root Image must receive track pointer input.");

            bgmSlider.value = 0.42f;
            sfxSlider.value = 0.75f;
            Assert.That(bgmValueText.text, Is.EqualTo("42%"));
            Assert.That(sfxValueText.text, Is.EqualTo("75%"));
            Assert.That(versionValueText.text, Is.EqualTo($"v{Application.version}"));

            var restoredSettings = new UserAudioSettings(bgmKey, sfxKey);
            Assert.That(restoredSettings.BgmVolume, Is.EqualTo(0.42f).Within(0.001f));
            Assert.That(restoredSettings.SfxVolume, Is.EqualTo(0.75f).Within(0.001f));

            exitPresenter.HandleBackRequested();
            Assert.That(optionsView.IsVisible, Is.False);
            Assert.That(exitView.IsVisible, Is.False);

            exitPresenter.HandleBackRequested();
            Assert.That(exitView.IsVisible, Is.True);

            exitPresenter.HandleBackRequested();
            optionsPresenter.Show();
            ((Button)serializedOptionsView.FindProperty("_closeButton").objectReferenceValue)
                .onClick.Invoke();
            Assert.That(optionsView.IsVisible, Is.False);
        }
        finally
        {
            exitPresenter.Dispose();
            optionsPresenter.Dispose();
            PlayerPrefs.DeleteKey(bgmKey);
            PlayerPrefs.DeleteKey(sfxKey);
            PlayerPrefs.Save();
        }
    }

    [Test]
    public void ExitPresenter_BackPriority_ClosesMenuBeforeOpeningPopup()
    {
        Scene scene = EditorSceneManager.OpenScene(
            "Assets/Scenes/Lobby.unity",
            OpenSceneMode.Single);
        ExitConfirmationView view = FindObjects<ExitConfirmationView>(scene)[0];
        LobbyBackInput backInput = FindObjects<LobbyBackInput>(scene)[0];
        SceneTransition sceneTransition = FindObjects<SceneTransition>(scene)[0];
        bool menuVisible = true;
        int menuCloseCount = 0;
        int quitCount = 0;
        var presenter = new LobbyExitPresenter(
            view,
            backInput,
            sceneTransition,
            () =>
            {
                if (!menuVisible)
                    return false;

                menuVisible = false;
                menuCloseCount++;
                return true;
            },
            () => quitCount++);

        try
        {
            Assert.That(view.IsVisible, Is.False);

            presenter.HandleBackRequested();
            Assert.That(menuVisible, Is.False);
            Assert.That(menuCloseCount, Is.EqualTo(1));
            Assert.That(view.IsVisible, Is.False);

            presenter.HandleBackRequested();
            Assert.That(view.IsVisible, Is.True);

            presenter.HandleBackRequested();
            Assert.That(view.IsVisible, Is.False);
            Assert.That(menuCloseCount, Is.EqualTo(1));

            presenter.HandleBackRequested();
            FindGameObject(scene, "CancelButton").GetComponent<Button>().onClick.Invoke();
            Assert.That(view.IsVisible, Is.False);
            Assert.That(quitCount, Is.Zero);

            presenter.HandleBackRequested();
            Button exitButton = FindGameObject(scene, "ExitButton").GetComponent<Button>();
            exitButton.onClick.Invoke();
            exitButton.onClick.Invoke();
            Assert.That(quitCount, Is.EqualTo(1));
            Assert.That(exitButton.interactable, Is.False);
            Assert.That(
                FindGameObject(scene, "CancelButton").GetComponent<Button>().interactable,
                Is.False);
        }
        finally
        {
            presenter.Dispose();
        }
    }

    private static List<T> FindObjects<T>(Scene scene) where T : Component
    {
        var results = new List<T>();

        foreach (GameObject root in scene.GetRootGameObjects())
            results.AddRange(root.GetComponentsInChildren<T>(true));

        return results;
    }

    private static GameObject FindGameObject(Scene scene, string name)
    {
        foreach (GameObject gameObject in GetAllGameObjects(scene))
        {
            if (gameObject.name == name)
                return gameObject;
        }

        return null;
    }

    private static List<GameObject> GetAllGameObjects(Scene scene)
    {
        var results = new List<GameObject>();

        foreach (GameObject root in scene.GetRootGameObjects())
        {
            foreach (Transform transform in root.GetComponentsInChildren<Transform>(true))
                results.Add(transform.gameObject);
        }

        return results;
    }

    private static List<string> GetDirectChildNames(Transform parent)
    {
        var results = new List<string>();
        foreach (Transform child in parent)
            results.Add(child.name);

        return results;
    }

    private static Vector2 GetScreenPosition(RectTransform rectTransform, float normalizedX)
    {
        Vector2 localPoint = new Vector2(
            Mathf.Lerp(rectTransform.rect.xMin, rectTransform.rect.xMax, normalizedX),
            rectTransform.rect.center.y);
        return RectTransformUtility.WorldToScreenPoint(null, rectTransform.TransformPoint(localPoint));
    }

    private static void AssertReferencesAssigned(Object target, params string[] propertyNames)
    {
        var serializedObject = new SerializedObject(target);

        foreach (string propertyName in propertyNames)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            Assert.That(property, Is.Not.Null, $"{target.name}.{propertyName} is missing.");
            Assert.That(
                property.objectReferenceValue,
                Is.Not.Null,
                $"{target.name}.{propertyName} is not assigned.");
        }
    }

    private static void AssertArrayAssigned(Object target, string propertyName, int expectedSize)
    {
        var serializedObject = new SerializedObject(target);
        SerializedProperty property = serializedObject.FindProperty(propertyName);
        Assert.That(property, Is.Not.Null);
        Assert.That(property.isArray, Is.True);
        Assert.That(property.arraySize, Is.EqualTo(expectedSize));

        for (int i = 0; i < property.arraySize; i++)
        {
            Assert.That(
                property.GetArrayElementAtIndex(i).objectReferenceValue,
                Is.Not.Null,
                $"{target.name}.{propertyName}[{i}] is not assigned.");
        }
    }

    private static string GetHierarchyPath(Transform transform)
    {
        string path = transform.name;

        while (transform.parent != null)
        {
            transform = transform.parent;
            path = $"{transform.name}/{path}";
        }

        return path;
    }
}
