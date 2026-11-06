using TMPro;
using UnityEngine;

public sealed class CoinsHUDView : MonoBehaviour
{
    [SerializeField] private TMP_Text _coinsAmountText;

    public bool IsConfigured => _coinsAmountText != null;

    public void SetCoins(int amount)
    {
        _coinsAmountText.text = $"+{Mathf.Max(0, amount):N0}";
    }
}
