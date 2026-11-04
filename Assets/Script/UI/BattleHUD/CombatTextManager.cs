using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public sealed class CombatTextManager : IUpdatable
{
    private readonly ObjectPool<FloatingCombatText> _textPool;
    private readonly RectTransform _root;
    private readonly Canvas _canvas;
    private readonly Camera _worldCamera;
    private readonly List<FloatingCombatText> _activeTexts = new(32);

    public CombatTextManager(
        ObjectPool<FloatingCombatText> textPool,
        RectTransform root,
        Camera worldCamera)
    {
        _textPool = textPool ?? throw new ArgumentNullException(nameof(textPool));
        _root = root != null ? root : throw new ArgumentNullException(nameof(root));
        _worldCamera = worldCamera != null ? worldCamera : throw new ArgumentNullException(nameof(worldCamera));
        _canvas = root.GetComponentInParent<Canvas>();

        if (_canvas == null)
            throw new ArgumentException("Combat text root must be placed under a Canvas.", nameof(root));
    }

    public void ShowDamage(Vector3 worldPosition, float amount)
    {
        if (amount > 0f)
            ShowAsync(worldPosition, -amount).Forget();
    }

    public void ShowHeal(Vector3 worldPosition, float amount)
    {
        if (amount > 0f)
            ShowAsync(worldPosition, amount).Forget();
    }

    public void Update(float deltaTime)
    {
        for (int i = _activeTexts.Count - 1; i >= 0; i--)
        {
            FloatingCombatText text = _activeTexts[i];

            if (text.Tick(deltaTime))
                continue;

            Release(i, text);
        }
    }

    private async UniTaskVoid ShowAsync(Vector3 worldPosition, float signedAmount)
    {
        Vector2 screenPosition = _worldCamera.WorldToScreenPoint(worldPosition);
        Camera eventCamera = _canvas.renderMode == RenderMode.ScreenSpaceOverlay
            ? null
            : _worldCamera;

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _root,
                screenPosition,
                eventCamera,
                out Vector2 anchoredPosition))
        {
            return;
        }

        FloatingCombatText text = await _textPool.GetAsync();
        text.transform.SetParent(_root, false);
        text.transform.SetAsLastSibling();
        text.Play(anchoredPosition, signedAmount);
        _activeTexts.Add(text);
    }

    private void Release(int index, FloatingCombatText text)
    {
        _activeTexts.RemoveAt(index);
        text.Clear();
        _textPool.Return(text);
    }
}
