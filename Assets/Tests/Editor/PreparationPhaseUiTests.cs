using System.Linq;
using NUnit.Framework;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public sealed class PreparationPhaseUiTests
{
    [TearDown]
    public void TearDown()
    {
        Time.timeScale = 1f;
    }

    [Test]
    public void AutoSkipSetting_DefaultsOff_AndOnlyPublishesChanges()
    {
        var setting = new PreparationAutoSkipSetting();
        int changeCount = 0;
        bool lastValue = false;
        setting.Changed += value =>
        {
            changeCount++;
            lastValue = value;
        };

        Assert.That(setting.IsEnabled, Is.False);

        setting.SetEnabled(false);
        setting.SetEnabled(true);
        setting.SetEnabled(true);

        Assert.That(setting.IsEnabled, Is.True);
        Assert.That(changeCount, Is.EqualTo(1));
        Assert.That(lastValue, Is.True);
    }

    [Test]
    public void BattleScene_HasSynchronizedAutoSkipTogglesAndPreparationCopy()
    {
        OpenBattleScene();

        GameObject bootstrapObject = FindSceneObject("GameBootstrapper");
        GameObject battleToggleObject = FindSceneObject("BattleAutoSkipToggle");
        GameObject rewardToggleObject = FindSceneObject("RewardAutoSkipToggle");
        GameObject rearrangementToggleObject = FindSceneObject("RearrangementAutoSkipToggle");
        GameObject speedObject = FindSceneObject("GameSpeedControl");
        GameObject preparationObject = FindSceneObject("CoreRearrangementUI");
        GameObject rewardPanelObject = FindSceneObject("RewardSelectionPanel");

        Assert.That(bootstrapObject, Is.Not.Null);
        Assert.That(battleToggleObject, Is.Not.Null);
        Assert.That(rewardToggleObject, Is.Not.Null);
        Assert.That(rearrangementToggleObject, Is.Not.Null);
        Assert.That(speedObject, Is.Not.Null);
        Assert.That(preparationObject, Is.Not.Null);
        Assert.That(rewardPanelObject, Is.Not.Null);

        PreparationAutoSkipView autoSkipView =
            bootstrapObject.GetComponent<PreparationAutoSkipView>();
        Assert.That(autoSkipView, Is.Not.Null);
        Assert.That(autoSkipView.IsConfigured, Is.True);

        var serializedBootstrap = new SerializedObject(
            bootstrapObject.GetComponent<GameBootstrapper>());
        Assert.That(
            serializedBootstrap.FindProperty("_preparationAutoSkipView").objectReferenceValue,
            Is.EqualTo(autoSkipView));

        Assert.That(battleToggleObject.transform.parent.name, Is.EqualTo("BattleHUDCanvas"));
        Assert.That(battleToggleObject.transform.parent, Is.EqualTo(speedObject.transform.parent));
        Assert.That(rewardToggleObject.transform.parent, Is.EqualTo(rewardPanelObject.transform));
        Assert.That(rearrangementToggleObject.transform.parent, Is.EqualTo(preparationObject.transform));

        RectTransform battleRect = battleToggleObject.GetComponent<RectTransform>();
        RectTransform speedRect = speedObject.GetComponent<RectTransform>();
        Assert.That(battleRect.anchorMin, Is.EqualTo(speedRect.anchorMin));
        Assert.That(battleRect.anchorMax, Is.EqualTo(speedRect.anchorMax));
        Assert.That(battleRect.pivot, Is.EqualTo(speedRect.pivot));
        Assert.That(battleRect.anchoredPosition.y, Is.EqualTo(speedRect.anchoredPosition.y));
        Assert.That(battleRect.anchoredPosition.x, Is.LessThan(speedRect.anchoredPosition.x));

        AssertToggleLabel(battleToggleObject, "Auto-Skip Prep Phase");
        AssertToggleLabel(rewardToggleObject, "Auto-Skip Prep Phase");
        AssertToggleLabel(rearrangementToggleObject, "Auto-Skip Next Prep Phase");
        AssertToggleTouchTarget(battleToggleObject);
        AssertToggleTouchTarget(rewardToggleObject);
        AssertToggleTouchTarget(rearrangementToggleObject);

        CoreRearrangementView preparationView =
            preparationObject.GetComponent<CoreRearrangementView>();
        Assert.That(preparationView, Is.Not.Null);
        Assert.That(preparationView.IsConfigured, Is.True);
        AssertText(preparationObject, "PreparationTitle", "PREPARATION PHASE");
        AssertText(preparationObject, "PreparationDescription", "Prepare for the next wave.");
        AssertText(preparationObject, "AutoAdvanceHint", "Next wave starts when time runs out.");
        AssertText(preparationObject, "ReadyHint", "When ready, tap READY to start the next wave.");
        AssertText(preparationObject, "ReadyButton/Label", "READY");

        preparationView.SetRemainingSeconds(9);
        AssertText(preparationObject, "TimerText", "00:09");
        preparationView.SetRemainingSeconds(0);
        AssertText(preparationObject, "TimerText", "00:00");
    }

    [Test]
    public void Presenters_ShowDisabledBattleControlsWhileInitializing_AndHideThemOutsideBattle()
    {
        OpenBattleScene();

        GameObject bootstrapObject = FindSceneObject("GameBootstrapper");
        GameObject speedObject = FindSceneObject("GameSpeedControl");
        GameObject battleToggleObject = FindSceneObject("BattleAutoSkipToggle");
        Toggle battleToggle = battleToggleObject.GetComponent<Toggle>();
        Toggle rewardToggle = FindSceneObject("RewardAutoSkipToggle").GetComponent<Toggle>();
        Toggle rearrangementToggle = FindSceneObject("RearrangementAutoSkipToggle").GetComponent<Toggle>();
        Button increaseButton = speedObject.transform.Find("IncreaseButton").GetComponent<Button>();
        var stateManager = new GameStateManager();
        var speedService = new GameTimePauseService(stateManager, new GameSpeedAccess(false));
        var speedPresenter = new GameSpeedPresenter(
            speedObject.GetComponent<GameSpeedView>(),
            speedService,
            stateManager);
        var setting = new PreparationAutoSkipSetting();
        var autoSkipPresenter = new PreparationAutoSkipPresenter(
            bootstrapObject.GetComponent<PreparationAutoSkipView>(),
            setting,
            stateManager);

        try
        {
            Assert.That(speedObject.activeSelf, Is.True);
            Assert.That(battleToggleObject.activeSelf, Is.True);
            Assert.That(increaseButton.interactable, Is.False);
            Assert.That(battleToggle.interactable, Is.False);
            Assert.That(speedService.TryIncreaseSpeed(), Is.False);

            stateManager.ChangeState(GameState.Playing);
            Assert.That(speedObject.activeSelf, Is.True);
            Assert.That(battleToggleObject.activeSelf, Is.True);
            Assert.That(increaseButton.interactable, Is.True);
            Assert.That(battleToggle.interactable, Is.True);
            Assert.That(speedService.TryIncreaseSpeed(), Is.True);
            Assert.That(speedService.CurrentSpeed, Is.EqualTo(2));

            speedObject.SetActive(false);
            battleToggleObject.SetActive(false);
            speedPresenter.Synchronize();
            autoSkipPresenter.Synchronize();
            Assert.That(speedObject.activeSelf, Is.True);
            Assert.That(battleToggleObject.activeSelf, Is.True);

            battleToggle.isOn = true;
            Assert.That(setting.IsEnabled, Is.True);
            Assert.That(rewardToggle.isOn, Is.True);
            Assert.That(rearrangementToggle.isOn, Is.True);

            stateManager.ChangeState(GameState.RewardSelection);
            Assert.That(speedObject.activeSelf, Is.False);
            Assert.That(battleToggleObject.activeSelf, Is.False);
            Assert.That(speedService.CurrentSpeed, Is.EqualTo(2));

            stateManager.ChangeState(GameState.Rearrangement);
            Assert.That(speedObject.activeSelf, Is.False);
            Assert.That(setting.IsEnabled, Is.True);

            stateManager.ChangeState(GameState.Playing);
            Assert.That(speedObject.activeSelf, Is.True);
            Assert.That(speedService.CurrentSpeed, Is.EqualTo(2));

            stateManager.ChangeState(GameState.Paused);
            Assert.That(speedObject.activeSelf, Is.False);
            Assert.That(battleToggleObject.activeSelf, Is.False);

            stateManager.ChangeState(GameState.Playing);
            Assert.That(speedObject.activeSelf, Is.True);
            Assert.That(battleToggleObject.activeSelf, Is.True);
            Assert.That(speedService.CurrentSpeed, Is.EqualTo(2));

            stateManager.ChangeState(GameState.GameOver);
            Assert.That(speedObject.activeSelf, Is.False);
            Assert.That(battleToggleObject.activeSelf, Is.False);
        }
        finally
        {
            autoSkipPresenter.Dispose();
            speedPresenter.Dispose();
            speedService.Dispose();
        }
    }

    [Test]
    public void SettingsOverlay_IsRaisedAboveCombatContent_WhileSettingsButtonRemainsAboveDim()
    {
        OpenBattleScene();

        GameObject canvasObject = FindSceneObject("BattleHUDCanvas");
        GameObject combatTextRootObject = FindSceneObject("CombatTextRoot");
        GameObject overlayObject = FindSceneObject("SettingsOverlay");
        GameObject settingsButtonObject = FindSceneObject("SettingsButton");
        GameObject bootstrapObject = FindSceneObject("GameBootstrapper");
        BattleSettingsView settingsView = canvasObject.GetComponent<BattleSettingsView>();
        var combatTextProbe = new GameObject("CombatTextProbe", typeof(RectTransform));
        combatTextProbe.transform.SetParent(combatTextRootObject.transform, false);

        var serializedBootstrap = new SerializedObject(
            bootstrapObject.GetComponent<GameBootstrapper>());
        Assert.That(
            serializedBootstrap.FindProperty("_combatTextRoot").objectReferenceValue,
            Is.EqualTo(combatTextRootObject.GetComponent<RectTransform>()));
        Assert.That(combatTextRootObject.transform.parent, Is.EqualTo(canvasObject.transform));
        Assert.That(
            combatTextRootObject.transform.GetSiblingIndex(),
            Is.LessThan(overlayObject.transform.GetSiblingIndex()));

        try
        {
            combatTextProbe.transform.SetAsLastSibling();
            settingsView.SetOpen(true);

            Assert.That(
                overlayObject.transform.GetSiblingIndex(),
                Is.GreaterThan(combatTextRootObject.transform.GetSiblingIndex()));
            Assert.That(
                settingsButtonObject.transform.GetSiblingIndex(),
                Is.GreaterThan(overlayObject.transform.GetSiblingIndex()));
        }
        finally
        {
            settingsView.SetOpen(false);
            Object.DestroyImmediate(combatTextProbe);
        }
    }

    [Test]
    public void BattleSettings_AudioSlidersUsePersistedSharedVolumeControls()
    {
        OpenBattleScene();

        Slider bgmSlider = FindSceneObject("BGMSlider").GetComponent<Slider>();
        Slider sfxSlider = FindSceneObject("SFXSlider").GetComponent<Slider>();

        Assert.That(bgmSlider.interactable, Is.True);
        Assert.That(sfxSlider.interactable, Is.True);
        Assert.That(bgmSlider.minValue, Is.Zero);
        Assert.That(bgmSlider.maxValue, Is.EqualTo(1f));
        Assert.That(sfxSlider.minValue, Is.Zero);
        Assert.That(sfxSlider.maxValue, Is.EqualTo(1f));
        Assert.That(bgmSlider.handleRect, Is.Not.Null);
        Assert.That(sfxSlider.handleRect, Is.Not.Null);
    }

    private static void OpenBattleScene()
    {
        EditorSceneManager.OpenScene("Assets/Scenes/Battle.unity", OpenSceneMode.Single);
    }

    private static GameObject FindSceneObject(string objectName)
    {
        Scene scene = SceneManager.GetActiveScene();

        foreach (GameObject root in scene.GetRootGameObjects())
        {
            Transform match = root.GetComponentsInChildren<Transform>(true)
                .FirstOrDefault(transform => transform.name == objectName);

            if (match != null)
                return match.gameObject;
        }

        return null;
    }

    private static void AssertToggleLabel(GameObject toggleObject, string expected)
    {
        TMP_Text label = toggleObject.transform.Find("Label").GetComponent<TMP_Text>();
        Assert.That(label.text, Is.EqualTo(expected));
    }

    private static void AssertToggleTouchTarget(GameObject toggleObject)
    {
        Toggle toggle = toggleObject.GetComponent<Toggle>();
        Image touchTarget = toggleObject.GetComponent<Image>();
        Image background = toggleObject.transform.Find("Background").GetComponent<Image>();
        Image checkmark = toggleObject.transform.Find("Background/Checkmark").GetComponent<Image>();
        TMP_Text label = toggleObject.transform.Find("Label").GetComponent<TMP_Text>();

        Assert.That(toggleObject.GetComponent<RectTransform>().rect.height, Is.GreaterThanOrEqualTo(48f));
        Assert.That(touchTarget, Is.Not.Null);
        Assert.That(touchTarget.color.a, Is.Zero);
        Assert.That(touchTarget.raycastTarget, Is.True);
        Assert.That(checkmark.raycastTarget, Is.False);
        Assert.That(label.raycastTarget, Is.False);
        Assert.That(toggle.targetGraphic, Is.EqualTo(background));
        Assert.That(toggle.graphic, Is.EqualTo(checkmark));
    }

    private static void AssertText(GameObject parent, string path, string expected)
    {
        TMP_Text text = parent.transform.Find(path).GetComponent<TMP_Text>();
        Assert.That(text.text, Is.EqualTo(expected));
    }
}
