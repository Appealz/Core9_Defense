using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

public sealed class CombatBoundsAndLaserTests
{
    private readonly List<Object> _createdObjects = new();

    [TearDown]
    public void TearDown()
    {
        for (int i = 0; i < _createdObjects.Count; i++)
        {
            if (_createdObjects[i] != null)
                Object.DestroyImmediate(_createdObjects[i]);
        }

        _createdObjects.Clear();
    }

    [TestCase(0f, 1f, 0f, 5f)]
    [TestCase(0f, -1f, 0f, -5f)]
    [TestCase(1f, 0f, 20f, 0f)]
    [TestCase(-1f, 0f, -20f, 0f)]
    [TestCase(1f, 1f, 5f, 5f)]
    [TestCase(-1f, -1f, -5f, -5f)]
    public void RayIntersection_ReachesCombatRectangleBoundary(
        float directionX,
        float directionY,
        float expectedX,
        float expectedY)
    {
        var bounds = new CombatBounds(-20f, 20f, -5f, 5f);

        bool found = bounds.TryGetRayIntersection(
            Vector2.zero,
            new Vector2(directionX, directionY),
            out Vector2 point);

        Assert.That(found, Is.True);
        Assert.That(point.x, Is.EqualTo(expectedX).Within(0.0001f));
        Assert.That(point.y, Is.EqualTo(expectedY).Within(0.0001f));
    }

    [Test]
    public void CombatBounds_UsesColliderOverlapAsVisibleCriterion()
    {
        var bounds = new CombatBounds(-5f, 5f, -5f, 5f);

        Assert.That(bounds.Intersects(new Bounds(new Vector3(5.4f, 0f), Vector3.one)), Is.True);
        Assert.That(bounds.Intersects(new Bounds(new Vector3(5.6f, 0f), Vector3.one)), Is.False);
    }

    [TestCase(900, 1600)]
    [TestCase(900, 1950)]
    [TestCase(900, 2000)]
    public void CameraAspectRatios_KeepTwentyFivePercentUiAndSeventyFivePercentCombat(
        int width,
        int height)
    {
        GameObject cameraObject = new("AspectCamera");
        _createdObjects.Add(cameraObject);
        Camera camera = cameraObject.AddComponent<Camera>();
        camera.orthographic = true;
        camera.orthographicSize = 20f;
        camera.transform.position = new Vector3(0f, 0f, -10f);
        var target = new RenderTexture(width, height, 0);
        _createdObjects.Add(target);
        camera.targetTexture = target;

        CameraBounds full = new CameraBoundsProvider(camera).GetBounds();
        CombatBounds combat = CombatBounds.FromCameraBounds(full, 0.25f);

        Assert.That(combat.Height / (full.Top - full.Bottom), Is.EqualTo(0.75f).Within(0.0001f));
        Assert.That(combat.Width / combat.Height,
            Is.EqualTo((width / (float)height) / 0.75f).Within(0.0001f));
    }

    [Test]
    public void SpawnCalculator_UsesCombatBoundaryAndPreservesMargin()
    {
        var bounds = new CombatBounds(-4f, 6f, -3f, 9f);
        var calculator = new SpawnPositionCalculator();

        Vector2 spawn = calculator.Calculate(bounds, new Vector2(1f, 3f), Vector2.down, 1.5f);

        Assert.That(spawn.x, Is.EqualTo(1f).Within(0.0001f));
        Assert.That(spawn.y, Is.EqualTo(-4.5f).Within(0.0001f));
    }

    [Test]
    public void CoreLayout_SetCenterAppliesOffsetMovesSlotsTogetherAndPreservesZ()
    {
        GameObject root = new("CoreLayoutTest");
        _createdObjects.Add(root);
        CoreLayout layout = root.AddComponent<CoreLayout>();
        var serializedLayout = new SerializedObject(layout);
        serializedLayout.FindProperty("_corePositionOffset").vector2Value = new Vector2(1.25f, -0.5f);
        serializedLayout.ApplyModifiedPropertiesWithoutUndo();
        root.transform.position = new Vector3(0f, 0f, 7f);
        Vector3 slotOffset = layout.GetSlotPosition(1) - root.transform.position;
        int changeCount = 0;
        layout.LayoutChanged += () => changeCount++;

        layout.SetCenter(new Vector2(3f, 4f));

        Assert.That(layout.PositionOffset, Is.EqualTo(new Vector2(1.25f, -0.5f)));
        Assert.That(root.transform.position, Is.EqualTo(new Vector3(4.25f, 3.5f, 7f)));
        Assert.That(layout.GetSlotPosition(1), Is.EqualTo(root.transform.position + slotOffset));
        Assert.That(changeCount, Is.EqualTo(1));
    }

    [Test]
    public void CombatLayoutCoordinator_RefreshesWhenOnlyInspectorOffsetChanges()
    {
        EditorSceneManager.OpenScene("Assets/Scenes/Battle.unity", OpenSceneMode.Single);
        CoreLayout layout = Object.FindFirstObjectByType<CoreLayout>();
        GameObject panelObject = GameObject.Find("CoreGoldUpgradePanel");
        Camera camera = Camera.main;
        Assert.That(layout, Is.Not.Null);
        Assert.That(panelObject, Is.Not.Null);
        Assert.That(camera, Is.Not.Null);

        Canvas.ForceUpdateCanvases();
        var boundsProvider = new CombatBoundsProvider(
            camera,
            panelObject.GetComponent<RectTransform>());
        var coordinator = new CombatLayoutCoordinator(boundsProvider, layout);
        Assert.That(coordinator.RefreshNow(), Is.True);
        CombatBounds bounds = boundsProvider.GetBounds();

        var serializedLayout = new SerializedObject(layout);
        serializedLayout.FindProperty("_corePositionOffset").vector2Value = new Vector2(0.75f, -0.5f);
        serializedLayout.ApplyModifiedPropertiesWithoutUndo();

        Assert.That(coordinator.RefreshNow(), Is.True);
        Assert.That((Vector2)layout.transform.position,
            Is.EqualTo(bounds.Center + new Vector2(0.75f, -0.5f)));
        Assert.That(coordinator.RefreshNow(), Is.False);
    }

    [Test]
    public void EnemyQuery_CombatPolicyIgnoresCloserEnemyOutsideBounds()
    {
        var enemyManager = new EnemyManager(null);
        Enemy outside = CreateAliveEnemy("Outside", new Vector2(5.6f, 0f));
        Enemy inside = CreateAliveEnemy("Inside", Vector2.zero);
        enemyManager.Register(outside);
        enemyManager.Register(inside);
        Physics2D.SyncTransforms();
        var query = new EnemyQuery(enemyManager);

        bool found = query.TryFindNearestInBounds(
            new Vector3(4.8f, 0f, 0f),
            new CombatBounds(-5f, 5f, -5f, 5f),
            out Enemy target);

        Assert.That(found, Is.True);
        Assert.That(target, Is.EqualTo(inside));
    }

    [Test]
    public void LaserVisual_UsesProvidedCombatBoundaryEndpoint()
    {
        GameObject laserObject = new("LaserTest");
        _createdObjects.Add(laserObject);
        LineRenderer lineRenderer = laserObject.AddComponent<LineRenderer>();
        Laser laser = laserObject.AddComponent<Laser>();
        typeof(Laser).GetMethod("Awake", BindingFlags.Instance | BindingFlags.NonPublic)
            .Invoke(laser, null);
        var bounds = new CombatBounds(-5f, 5f, -4f, 6f);
        Vector3 end = new(5f, 2.5f, 0f);

        laser.Initialize(Vector3.zero, end, bounds, 10f, 0.3f, 0.5f, 0.1f);

        Assert.That(lineRenderer.positionCount, Is.EqualTo(5));
        Assert.That(lineRenderer.GetPosition(4), Is.EqualTo(end));
        Assert.That(typeof(ITowerTargetingPolicy).IsAssignableFrom(typeof(LaserAttack)), Is.True);
    }

    [Test]
    public void BattleScene_PanelAndCalculatedCombatBoundsUseSeventyFivePercentHeight()
    {
        EditorSceneManager.OpenScene("Assets/Scenes/Battle.unity", OpenSceneMode.Single);
        GameObject panelObject = GameObject.Find("CoreGoldUpgradePanel");
        Camera camera = Camera.main;
        Assert.That(panelObject, Is.Not.Null);
        Assert.That(camera, Is.Not.Null);

        RectTransform panelRect = panelObject.GetComponent<RectTransform>();
        Assert.That(panelRect.anchorMax.y, Is.EqualTo(0.25f));
        Canvas.ForceUpdateCanvases();

        var fullBoundsProvider = new CameraBoundsProvider(camera);
        var combatBoundsProvider = new CombatBoundsProvider(camera, panelRect);
        CameraBounds full = fullBoundsProvider.GetBounds();
        CombatBounds combat = combatBoundsProvider.GetBounds();

        Assert.That(combat.Height / (full.Top - full.Bottom), Is.EqualTo(0.75f).Within(0.001f));
        Assert.That(combat.Center.y, Is.EqualTo((combat.Bottom + combat.Top) * 0.5f));

        panelObject.SetActive(false);
        CombatBounds hiddenPanelCombat = combatBoundsProvider.GetBounds();
        panelObject.SetActive(true);
        Assert.That(hiddenPanelCombat.Bottom, Is.EqualTo(combat.Bottom).Within(0.0001f));
        Assert.That(hiddenPanelCombat.Top, Is.EqualTo(combat.Top).Within(0.0001f));
    }

    private Enemy CreateAliveEnemy(string name, Vector2 position)
    {
        GameObject enemyObject = new(name);
        _createdObjects.Add(enemyObject);
        enemyObject.transform.position = position;
        enemyObject.AddComponent<BoxCollider2D>();
        Enemy enemy = enemyObject.AddComponent<Enemy>();
        PropertyInfo healthProperty = typeof(Enemy).GetProperty(
            nameof(Enemy.Health),
            BindingFlags.Instance | BindingFlags.Public);
        healthProperty.SetValue(enemy, new EnemyHealth(100f));
        return enemy;
    }
}
