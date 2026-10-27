using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RewardSelectionView : MonoBehaviour
{
    [SerializeField] private RewardCardView _cardPrefab;
    [SerializeField] private Transform _cardContainer;
    [SerializeField] private TMP_Text _selectionProgressText;

    public event Action<RewardData> RewardSelected;

    private readonly List<RewardCardView> _cards = new();

    public void Show(IReadOnlyList<RewardData> rewards, int currentSelection, int totalSelections)
    {
        gameObject.SetActive(true);
        EnsureCardCount(rewards.Count);

        if (_selectionProgressText != null)
            _selectionProgressText.text = $"Reward Selection {currentSelection} / {totalSelections}";

        for (int i = 0; i < _cards.Count; i++)
        {
            bool isActive = i < rewards.Count;
            _cards[i].gameObject.SetActive(isActive);

            if (isActive)
                _cards[i].Bind(rewards[i]);
        }
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    private void EnsureCardCount(int count)
    {
        while (_cards.Count < count)
        {
            RewardCardView card = Instantiate(_cardPrefab, _cardContainer);
            card.Selected += OnRewardSelected;
            _cards.Add(card);
        }
    }

    private void OnRewardSelected(RewardData rewardData)
    {
        foreach (RewardCardView card in _cards)
            card.SetInteractable(false);

        RewardSelected?.Invoke(rewardData);
    }

    private void OnDestroy()
    {
        foreach (RewardCardView card in _cards)
        {
            if (card != null)
                card.Selected -= OnRewardSelected;
        }
    }
}
