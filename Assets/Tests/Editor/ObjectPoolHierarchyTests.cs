using System.Collections;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public sealed class ObjectPoolHierarchyTests
{
    private static readonly string[] PoolRootNames =
    {
        "Enemies",
        "Projectiles",
        "Explosions",
        "Lasers"
    };

    [Test]
    public void BattleScene_HasOneConfiguredObjectPoolsHierarchy()
    {
        Scene scene = EditorSceneManager.OpenScene("Assets/Scenes/Battle.unity", OpenSceneMode.Single);
        GameObject[] sceneRoots = scene.GetRootGameObjects();
        GameObject objectPools = null;
        int objectPoolsCount = 0;

        for (int i = 0; i < sceneRoots.Length; i++)
        {
            if (sceneRoots[i].name != "ObjectPools")
                continue;

            objectPools = sceneRoots[i];
            objectPoolsCount++;
        }

        Assert.That(objectPoolsCount, Is.EqualTo(1));
        AssertIdentity(objectPools.transform);
        Assert.That(objectPools.transform.childCount, Is.EqualTo(PoolRootNames.Length));

        for (int i = 0; i < PoolRootNames.Length; i++)
        {
            Transform poolRoot = objectPools.transform.Find(PoolRootNames[i]);
            Assert.That(poolRoot, Is.Not.Null, $"{PoolRootNames[i]} pool root is missing.");
            Assert.That(poolRoot.parent, Is.SameAs(objectPools.transform));
            AssertIdentity(poolRoot);
        }

        GameBootstrapper bootstrapper = Object.FindAnyObjectByType<GameBootstrapper>();
        Assert.That(bootstrapper, Is.Not.Null);

        var serializedBootstrapper = new SerializedObject(bootstrapper);
        Transform configuredRoot = (Transform)serializedBootstrapper
            .FindProperty("_objectPoolsRoot")
            .objectReferenceValue;
        Assert.That(configuredRoot, Is.SameAs(objectPools.transform));
    }

    [UnityTest]
    public IEnumerator BattlePlayMode_PrewarmCreatesObjectsUnderConfiguredRoots()
    {
        EditorSceneManager.OpenScene("Assets/Scenes/Battle.unity", OpenSceneMode.Single);
        yield return new EnterPlayMode();

        GameObject objectPools = GameObject.Find("ObjectPools");
        float deadline = Time.realtimeSinceStartup + 15f;

        while (!HasExpectedPrewarmCounts(objectPools) && Time.realtimeSinceStartup < deadline)
            yield return null;

        Assert.That(objectPools, Is.Not.Null);
        Assert.That(objectPools.transform.Find("Enemies").childCount, Is.GreaterThanOrEqualTo(10));
        Assert.That(objectPools.transform.Find("Projectiles").childCount, Is.GreaterThanOrEqualTo(30));
        Assert.That(objectPools.transform.Find("Explosions").childCount, Is.GreaterThanOrEqualTo(10));
        Assert.That(objectPools.transform.Find("Lasers").childCount, Is.GreaterThanOrEqualTo(10));

        yield return new ExitPlayMode();
    }

    [Test]
    public void ObjectPool_OverflowAndReuseKeepTheConfiguredParent()
    {
        var parent = new GameObject("PoolParent");
        var prefab = new GameObject("PooledObject");
        prefab.AddComponent<PoolHierarchyTestComponent>();
        int createdCount = 0;

        var pool = new ObjectPool<PoolHierarchyTestComponent>(
            () =>
            {
                createdCount++;
                GameObject instance = Object.Instantiate(prefab, parent.transform, false);
                return UniTask.FromResult(instance.GetComponent<PoolHierarchyTestComponent>());
            },
            onGet: item => item.gameObject.SetActive(true),
            onReturn: item => item.gameObject.SetActive(false));

        try
        {
            pool.PrewarmAsync(1).GetAwaiter().GetResult();
            PoolHierarchyTestComponent prewarmed = pool.GetAsync().GetAwaiter().GetResult();
            PoolHierarchyTestComponent overflow = pool.GetAsync().GetAwaiter().GetResult();

            Assert.That(createdCount, Is.EqualTo(2));
            Assert.That(prewarmed.transform.parent, Is.SameAs(parent.transform));
            Assert.That(overflow.transform.parent, Is.SameAs(parent.transform));

            pool.Return(prewarmed);
            pool.Return(overflow);
            PoolHierarchyTestComponent reused = pool.GetAsync().GetAwaiter().GetResult();

            Assert.That(createdCount, Is.EqualTo(2));
            Assert.That(reused.transform.parent, Is.SameAs(parent.transform));
        }
        finally
        {
            Object.DestroyImmediate(parent);
            Object.DestroyImmediate(prefab);
        }
    }

    private static void AssertIdentity(Transform target)
    {
        Assert.That(target.localPosition, Is.EqualTo(Vector3.zero));
        Assert.That(target.localRotation, Is.EqualTo(Quaternion.identity));
        Assert.That(target.localScale, Is.EqualTo(Vector3.one));
    }

    private static bool HasExpectedPrewarmCounts(GameObject objectPools)
    {
        if (objectPools == null)
            return false;

        Transform enemies = objectPools.transform.Find("Enemies");
        Transform projectiles = objectPools.transform.Find("Projectiles");
        Transform explosions = objectPools.transform.Find("Explosions");
        Transform lasers = objectPools.transform.Find("Lasers");

        return enemies != null && enemies.childCount >= 10 &&
               projectiles != null && projectiles.childCount >= 30 &&
               explosions != null && explosions.childCount >= 10 &&
               lasers != null && lasers.childCount >= 10;
    }
}

public sealed class PoolHierarchyTestComponent : MonoBehaviour
{
}
