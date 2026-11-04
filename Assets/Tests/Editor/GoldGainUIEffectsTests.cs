using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class GoldGainUIEffectsTests
{
    [Test]
    public void GoldManager_GainAndSpendEventsAreIndividualAndOnlyPublishActualChanges()
    {
        var goldManager = new GoldManager();
        var gains = new List<int>();
        var spends = new List<int>();
        var eventOrder = new List<string>();

        goldManager.GoldChanged += amount => eventOrder.Add($"changed:{amount}");
        goldManager.GoldGained += amount =>
        {
            gains.Add(amount);
            eventOrder.Add($"gained:{amount}");
        };
        goldManager.GoldSpent += amount =>
        {
            spends.Add(amount);
            eventOrder.Add($"spent:{amount}");
        };

        goldManager.AddGold(3);
        goldManager.AddGold(5);
        goldManager.AddGold(2);
        Assert.That(goldManager.TrySpendGold(4), Is.True);
        Assert.That(goldManager.TrySpendGold(100), Is.False);

        Assert.That(gains, Is.EqualTo(new[] { 3, 5, 2 }));
        Assert.That(spends, Is.EqualTo(new[] { 4 }));
        Assert.That(eventOrder, Is.EqualTo(new[]
        {
            "changed:3", "gained:3",
            "changed:8", "gained:5",
            "changed:10", "gained:2",
            "changed:6", "spent:4"
        }));

        var saturatedManager = new GoldManager();
        int saturatedGainEventCount = 0;
        saturatedManager.GoldGained += _ => saturatedGainEventCount++;
        saturatedManager.AddGold(int.MaxValue);
        saturatedManager.AddGold(1);

        Assert.That(saturatedManager.CurrentGold, Is.EqualTo(int.MaxValue));
        Assert.That(saturatedGainEventCount, Is.EqualTo(1));
    }

    [Test]
    public void GoldGainPopup_UsesSerializedDefaultsAndResetsWhenReused()
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(
            "Assets/Prefabs/GoldGainPopup.prefab");

        Assert.That(prefab, Is.Not.Null);
        Assert.That(prefab.GetComponent<TextMeshProUGUI>(), Is.Not.Null);
        Assert.That(prefab.GetComponent<CanvasGroup>(), Is.Not.Null);

        GoldGainPopup prefabPopup = prefab.GetComponent<GoldGainPopup>();
        Assert.That(prefabPopup, Is.Not.Null);

        var serializedObject = new SerializedObject(prefabPopup);
        Assert.That(serializedObject.FindProperty("_fontSize").floatValue, Is.EqualTo(30f));
        Assert.That(serializedObject.FindProperty("_gainColor"), Is.Not.Null);
        Assert.That(serializedObject.FindProperty("_displayDuration").floatValue, Is.EqualTo(0.35f));
        Assert.That(serializedObject.FindProperty("_floatDistance").floatValue, Is.EqualTo(35f));
        Assert.That(serializedObject.FindProperty("_randomOffset").floatValue, Is.EqualTo(8f));
        Assert.That(serializedObject.FindProperty("_spendColor"), Is.Not.Null);
        Assert.That(serializedObject.FindProperty("_spendFontSize").floatValue, Is.EqualTo(30f));
        Assert.That(serializedObject.FindProperty("_spendSpawnOffset").vector2Value, Is.EqualTo(new Vector2(0f, -25f)));
        Assert.That(serializedObject.FindProperty("_spendDisplayDuration").floatValue, Is.EqualTo(0.35f));
        Assert.That(serializedObject.FindProperty("_spendFloatDistance").floatValue, Is.EqualTo(35f));
        Assert.That(serializedObject.FindProperty("_spendRandomOffset").floatValue, Is.EqualTo(8f));

        GameObject instance = Object.Instantiate(prefab);

        try
        {
            GoldGainPopup popup = instance.GetComponent<GoldGainPopup>();
            TextMeshProUGUI text = instance.GetComponent<TextMeshProUGUI>();
            CanvasGroup canvasGroup = instance.GetComponent<CanvasGroup>();
            RectTransform rectTransform = instance.GetComponent<RectTransform>();

            popup.Play(Vector2.zero, 3);
            float startY = rectTransform.anchoredPosition.y;
            Assert.That(text.text, Is.EqualTo("+3"));
            Assert.That(popup.Tick(0.175f), Is.True);
            Assert.That(rectTransform.anchoredPosition.y, Is.EqualTo(startY + 17.5f).Within(0.001f));
            Assert.That(canvasGroup.alpha, Is.EqualTo(0.5f).Within(0.001f));
            Assert.That(popup.Tick(0.175f), Is.False);

            popup.Clear();
            popup.Play(Vector2.zero, 5);

            Assert.That(text.text, Is.EqualTo("+5"));
            Assert.That(canvasGroup.alpha, Is.EqualTo(1f));
            Assert.That(rectTransform.localScale, Is.EqualTo(Vector3.one));

            popup.Clear();
            popup.PlaySpend(Vector2.zero, 2);
            float spendStartY = rectTransform.anchoredPosition.y;

            Assert.That(text.text, Is.EqualTo("-2"));
            Assert.That(spendStartY, Is.LessThan(0f));
            Assert.That(text.color.r, Is.GreaterThan(text.color.g));
            Assert.That(popup.Tick(0.175f), Is.True);
            Assert.That(rectTransform.anchoredPosition.y, Is.EqualTo(spendStartY - 17.5f).Within(0.001f));
        }
        finally
        {
            Object.DestroyImmediate(instance);
        }
    }

    [Test]
    public void BattleGoldHud_GainAndSpendUseIndependentEffectsAndReusePool()
    {
        EditorSceneManager.OpenScene("Assets/Scenes/Battle.unity", OpenSceneMode.Single);
        GoldHUDView view = Object.FindAnyObjectByType<GoldHUDView>();

        Assert.That(view, Is.Not.Null);
        Assert.That(view.IsConfigured, Is.True);

        var serializedView = new SerializedObject(view);
        Assert.That(serializedView.FindProperty("_punchScale").floatValue, Is.EqualTo(1.15f));
        Assert.That(serializedView.FindProperty("_punchDuration").floatValue, Is.EqualTo(0.18f));
        Assert.That(serializedView.FindProperty("_countDuration").floatValue, Is.EqualTo(0.3f));
        Assert.That(serializedView.FindProperty("_countEaseOutPower").floatValue, Is.EqualTo(3f));

        TMP_Text amountText = (TMP_Text)serializedView.FindProperty("_goldAmountText").objectReferenceValue;
        RectTransform popupRoot = (RectTransform)serializedView.FindProperty("_gainPopupRoot").objectReferenceValue;
        Vector3 baseScale = amountText.rectTransform.localScale;
        var goldManager = new GoldManager();
        using var presenter = new GoldHUDPresenter(view, goldManager);

        goldManager.AddGold(1000);
        int childCountAfterPrewarm = popupRoot.childCount;
        Assert.That(amountText.text, Is.EqualTo("0"));
        Assert.That(GetActivePopupCount(view), Is.EqualTo(1));

        InvokePrivate(view, "UpdateGoldCount", 0.1f);
        int gainIntermediate = ParseGold(amountText.text);
        Assert.That(gainIntermediate, Is.GreaterThan(0).And.LessThan(1000));

        InvokePrivate(view, "UpdateGoldCount", 0.2f);
        Assert.That(amountText.text, Is.EqualTo("1,000"));

        InvokePrivate(view, "UpdatePunch", 0.09f);
        Assert.That(amountText.rectTransform.localScale.x, Is.GreaterThan(baseScale.x));

        Assert.That(goldManager.TrySpendGold(250), Is.True);
        Assert.That(amountText.text, Is.EqualTo("1,000"));
        Assert.That(GetActivePopupCount(view), Is.EqualTo(2));

        InvokePrivate(view, "UpdateGoldCount", 0.1f);
        int spendIntermediate = ParseGold(amountText.text);
        Assert.That(spendIntermediate, Is.GreaterThan(750).And.LessThan(1000));

        goldManager.AddGold(500);
        Assert.That(ParseGold(amountText.text), Is.EqualTo(spendIntermediate));
        Assert.That(GetActivePopupCount(view), Is.EqualTo(3));
        Assert.That(amountText.rectTransform.localScale, Is.EqualTo(baseScale));

        InvokePrivate(view, "UpdateGoldCount", 0.1f);
        int redirectedIntermediate = ParseGold(amountText.text);
        Assert.That(redirectedIntermediate, Is.GreaterThan(spendIntermediate).And.LessThan(1250));

        InvokePrivate(view, "UpdateGoldCount", 0.2f);
        Assert.That(amountText.text, Is.EqualTo("1,250"));

        Assert.That(goldManager.TrySpendGold(1250), Is.True);
        InvokePrivate(view, "UpdateGoldCount", 0.3f);
        Assert.That(amountText.text, Is.EqualTo("0"));
        Assert.That(GetActivePopupCount(view), Is.EqualTo(4));

        Assert.That(goldManager.TrySpendGold(1), Is.False);
        Assert.That(amountText.text, Is.EqualTo("0"));
        Assert.That(GetActivePopupCount(view), Is.EqualTo(4));

        InvokePrivate(view, "OnDisable");
        view.PlayGoldGain(2);

        Assert.That(popupRoot.childCount, Is.EqualTo(childCountAfterPrewarm));
        Assert.That(GetActivePopupCount(view), Is.EqualTo(1));
    }

    private static int ParseGold(string text)
    {
        return int.Parse(text.Replace(",", string.Empty));
    }

    private static int GetActivePopupCount(GoldHUDView view)
    {
        FieldInfo field = typeof(GoldHUDView).GetField(
            "_activePopups",
            BindingFlags.Instance | BindingFlags.NonPublic);
        return ((ICollection)field.GetValue(view)).Count;
    }

    private static void InvokePrivate(GoldHUDView view, string methodName, params object[] arguments)
    {
        MethodInfo method = typeof(GoldHUDView).GetMethod(
            methodName,
            BindingFlags.Instance | BindingFlags.NonPublic);
        method.Invoke(view, arguments);
    }
}
