
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
