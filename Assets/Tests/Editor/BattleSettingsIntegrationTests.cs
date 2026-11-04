using System.Collections;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Object = UnityEngine.Object;

public sealed class BattleSettingsIntegrationTests
{
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
