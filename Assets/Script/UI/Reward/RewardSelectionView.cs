using System;
using System.Collections.Generic;
using UnityEngine;

public class RewardSelectionView : MonoBehaviour
{
    [SerializeField] private RewardCardView _cardPrefab;
    [SerializeField] private Transform _cardContainer;

    public event Action<RewardData> RewardSelected;

    private readonly List<RewardCardView> _cards = new();

    public void Show(IReadOnlyList<RewardData> rewards)
    {
        gameObject.SetActive(true);
        EnsureCardCount(rewards.Count);

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
