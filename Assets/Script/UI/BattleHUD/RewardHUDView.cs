using TMPro;
using UnityEngine;

public sealed class RewardHUDView : MonoBehaviour
{
    [SerializeField] private TMP_Text _rewardAmountText;

    public void SetRewardCount(int amount)
    {
        _rewardAmountText.text = $"Reward {amount}";
    }
}
