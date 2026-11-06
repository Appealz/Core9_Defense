using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Object = UnityEngine.Object;

public sealed class BattleSettingsIntegrationTests
{
    [Test]
    public void BattleScene_HasConfiguredEarlyExitConfirmationPopup()
    {
        Scene scene = EditorSceneManager.OpenScene(
            "Assets/Scenes/Battle.unity",
            OpenSceneMode.Single);
        BattleExitConfirmationView popup = Resources
            .FindObjectsOfTypeAll<BattleExitConfirmationView>()
            .FirstOrDefault(candidate => candidate.gameObject.scene == scene);

        Assert.That(popup, Is.Not.Null);
        Assert.That(popup.IsConfigured, Is.True);
        Assert.That(popup.gameObject.name, Is.EqualTo("EarlyExitConfirmationRoot"));
        Assert.That(popup.gameObject.activeSelf, Is.False);
        Assert.That(popup.transform.parent.name, Is.EqualTo("SettingsOverlay"));

        GameBootstrapper bootstrapper = Object.FindAnyObjectByType<GameBootstrapper>();
        Assert.That(bootstrapper, Is.Not.Null);

        var serializedBootstrapper = new SerializedObject(bootstrapper);
        Assert.That(
            serializedBootstrapper.FindProperty("_exitConfirmationView").objectReferenceValue,
            Is.SameAs(popup));
        Assert.That(
            serializedBootstrapper.FindProperty("_earlyExitCoinsPenaltyRate").floatValue,
            Is.EqualTo(0.2f).Within(0.0001f));
    }

    [UnityTest]
    public IEnumerator SettingsButton_PausesBattle_AndResumeRestoresIt()
    {
        EditorSceneManager.OpenScene("Assets/Scenes/Battle.unity", OpenSceneMode.Single);
        yield return new EnterPlayMode();

        BattleSettingsView view = null;
        Button settingsButton = null;
        float startupDeadline = Time.realtimeSinceStartup + 15f;

        while ((view == null || settingsButton == null || !settingsButton.interactable) &&
               Time.realtimeSinceStartup < startupDeadline)
        {
            view = Object.FindAnyObjectByType<BattleSettingsView>();

            if (view != null)
                settingsButton = view.transform.Find("SettingsButton")?.GetComponent<Button>();

            yield return null;
        }

        Assert.That(view, Is.Not.Null);
        Assert.That(settingsButton, Is.Not.Null);
        Assert.That(settingsButton.interactable, Is.True);

        Transform overlay = view.transform.Find("SettingsOverlay");
        Button resumeButton = overlay
            .Find("SettingsPanel/ResumeGameButton")
            .GetComponent<Button>();

        settingsButton.onClick.Invoke();
        yield return null;

        Assert.That(overlay.gameObject.activeSelf, Is.True);
        Assert.That(Time.timeScale, Is.Zero);

        resumeButton.onClick.Invoke();
        yield return null;

        Assert.That(overlay.gameObject.activeSelf, Is.False);
        Assert.That(Time.timeScale, Is.EqualTo(1f));
        Assert.That(settingsButton.interactable, Is.True);

        yield return new ExitPlayMode();
    }
}
