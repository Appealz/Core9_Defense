using System.Collections.Generic;
using NUnit.Framework;
using TMPro;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEngine;

public sealed class FloatingCombatTextTests
{
    [Test]
    public void EnemyHealth_EachDamageEmitsFinalDamage_IncludingLethalHit()
    {
        var health = new EnemyHealth(100f);
        var damages = new List<float>();
        var order = new List<string>();

        health.Damaged += amount =>
        {
            damages.Add(amount);
            order.Add("damaged");
        };
        health.Died += () => order.Add("died");

        health.TakeDamage(30f);
        health.TakeDamage(40f);
        health.TakeDamage(100f);

        Assert.That(damages, Is.EqualTo(new[] { 30f, 40f, 100f }));
        Assert.That(order, Is.EqualTo(new[] { "damaged", "damaged", "damaged", "died" }));
        Assert.That(health.CurrentHp, Is.Zero);
    }

    [Test]
    public void EnemyHealth_HealEmitsOnlyActualIncrease_AndFullHpAttemptEmitsNothing()
    {
        var health = new EnemyHealth(100f);
        var healingAmounts = new List<float>();
        health.Healed += healingAmounts.Add;

        health.TakeDamage(35f);
        bool firstHealSucceeded = health.TryHeal(20f);
        bool cappedHealSucceeded = health.TryHeal(50f);
        bool fullHpHealSucceeded = health.TryHeal(10f);

        Assert.That(firstHealSucceeded, Is.True);
        Assert.That(cappedHealSucceeded, Is.True);
        Assert.That(fullHpHealSucceeded, Is.False);
        Assert.That(healingAmounts, Is.EqualTo(new[] { 20f, 15f }));
        Assert.That(health.CurrentHp, Is.EqualTo(100f));
    }

    [Test]
    public void FloatingCombatTextPrefab_HasRequiredComponentsAndSerializedDefaults()
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(
            "Assets/Prefabs/FloatingCombatText.prefab");

        Assert.That(prefab, Is.Not.Null);
        Assert.That(prefab.GetComponent<TextMeshProUGUI>(), Is.Not.Null);
        Assert.That(prefab.GetComponent<CanvasGroup>(), Is.Not.Null);

        FloatingCombatText floatingText = prefab.GetComponent<FloatingCombatText>();
        Assert.That(floatingText, Is.Not.Null);

        var serializedObject = new SerializedObject(floatingText);
        Assert.That(serializedObject.FindProperty("_fontSize").floatValue, Is.EqualTo(24f));
        Assert.That(serializedObject.FindProperty("_duration").floatValue, Is.EqualTo(0.7f));
        Assert.That(serializedObject.FindProperty("_floatDistance").floatValue, Is.EqualTo(50f));
        Assert.That(serializedObject.FindProperty("_randomOffset").floatValue, Is.EqualTo(15f));
        Assert.That(serializedObject.FindProperty("_damageColor"), Is.Not.Null);
        Assert.That(serializedObject.FindProperty("_healColor"), Is.Not.Null);
        Assert.That(serializedObject.FindProperty("_outlineColor"), Is.Not.Null);
        Assert.That(serializedObject.FindProperty("_outlineWidth"), Is.Not.Null);
        Assert.That(serializedObject.FindProperty("_fadeStart"), Is.Not.Null);
        Assert.That(serializedObject.FindProperty("_fadePower"), Is.Not.Null);
    }

    [Test]
    public void FloatingCombatTextPrefab_IsRegisteredWithExpectedAddress()
    {
        string guid = AssetDatabase.AssetPathToGUID("Assets/Prefabs/FloatingCombatText.prefab");
        var entry = AddressableAssetSettingsDefaultObject.Settings.FindAssetEntry(guid);

        Assert.That(entry, Is.Not.Null);
        Assert.That(entry.address, Is.EqualTo(AddressableKeys.FloatingCombatTextPrefab));
    }

    [Test]
    public void FloatingCombatText_ReusedInstanceResetsValueAndAlpha()
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(
            "Assets/Prefabs/FloatingCombatText.prefab");
        GameObject instance = Object.Instantiate(prefab);

        try
        {
            FloatingCombatText floatingText = instance.GetComponent<FloatingCombatText>();
            TextMeshProUGUI text = instance.GetComponent<TextMeshProUGUI>();
            CanvasGroup canvasGroup = instance.GetComponent<CanvasGroup>();
            RectTransform rectTransform = instance.GetComponent<RectTransform>();

            floatingText.Play(Vector2.zero, -30f);
            float startY = rectTransform.anchoredPosition.y;
            Assert.That(text.text, Is.EqualTo("-30"));
            Assert.That(canvasGroup.alpha, Is.EqualTo(1f));
            Assert.That(floatingText.Tick(0.35f), Is.True);
            Assert.That(rectTransform.anchoredPosition.y, Is.EqualTo(startY + 25f).Within(0.001f));
            Assert.That(canvasGroup.alpha, Is.InRange(0f, 1f).And.LessThan(1f));
            Assert.That(floatingText.Tick(0.35f), Is.False);

            floatingText.Clear();
            floatingText.Play(Vector2.zero, 20f);

            Assert.That(text.text, Is.EqualTo("+20"));
            Assert.That(canvasGroup.alpha, Is.EqualTo(1f));
            Assert.That(floatingText.Tick(0f), Is.True);
        }
        finally
        {
            Object.DestroyImmediate(instance);
        }
    }
}
