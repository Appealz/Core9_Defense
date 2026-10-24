
using TMPro;
using UnityEngine;

public class GoldHUDView : MonoBehaviour
{
    [SerializeField] private TMP_Text _goldAmountText;

    public void SetGold(int amount)
    {
        _goldAmountText.text = amount.ToString("N0");
    }
}
