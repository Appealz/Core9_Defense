
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class RewardCardView : MonoBehaviour
{
    [SerializeField] private Image _icon;
    [SerializeField] private TMP_Text _rewardName;
    [SerializeField] private TMP_Text _description;
    [SerializeField] private TMP_Text _rarity;
    [SerializeField] private Image _rarityBackground;
    [SerializeField] private RewardRarityColorPalette _rarityColors = new();

    public event Action<RewardData> Selected;

    private Button _button;
    private RewardData _rewardData;

    private void Awake()
    {
        _button = GetComponent<Button>();
        _button.onClick.AddListener(OnClicked);
    }

    public void Bind(RewardData rewardData)
    {
        _rewardData = rewardData;

        _icon.sprite = rewardData.Icon;
        _rewardName.text = rewardData.RewardName;
        _description.text = rewardData.Description;

        if (_rarity != null)
            _rarity.text = rewardData.Rarity.ToString();

        if (_rarityBackground != null && _rarityColors != null)
            _rarityBackground.color = _rarityColors.GetColor(rewardData.Rarity);

        SetInteractable(true);
    }

    public void SetInteractable(bool interactable)
    {
        if (_button == null)
            _button = GetComponent<Button>();

        _button.interactable = interactable;
    }

    private void OnClicked()
    {
        if (_rewardData == null)
            return;

        Selected?.Invoke(_rewardData);
    }

    private void OnDestroy()
    {
        if (_button != null)
            _button.onClick.RemoveListener(OnClicked);
    }
}
