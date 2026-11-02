using System.Collections.Generic;
using NUnit.Framework;
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
        Assert.That(FindObjects<Canvas>(scene), Has.Count.EqualTo(1));
        Assert.That(FindObjects<EventSystem>(scene), Has.Count.EqualTo(1));
        Assert.That(FindGameObject(scene, "LobbyCanvas"), Is.Not.Null);
        Assert.That(FindGameObject(scene, "LobbyMain"), Is.Not.Null);
        Assert.That(FindGameObject(scene, "UpgradeScreen"), Is.Not.Null);

        LobbySceneController controller = FindObjects<LobbySceneController>(scene)[0];
        LobbyView lobbyView = FindObjects<LobbyView>(scene)[0];
        UpgradeView upgradeView = FindObjects<UpgradeView>(scene)[0];
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
