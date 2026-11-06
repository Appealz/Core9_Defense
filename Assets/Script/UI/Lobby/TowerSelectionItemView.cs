using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class TowerSelectionItemView : MonoBehaviour
{
    private static readonly Color SelectedBackground = new Color(0.96f, 0.97f, 1f, 1f);
    private static readonly Color IdleBackground = new Color(0.13f, 0.16f, 0.22f, 1f);
    private static readonly Color SelectedText = new Color(0.04f, 0.06f, 0.1f, 1f);

    [SerializeField] private TowerPartData _towerPart;
    [SerializeField] private Button _button;
    [SerializeField] private Image _background;
    [SerializeField] private Image _towerImage;
    [SerializeField] private TMP_Text _nameText;

    private bool _initialized;

    public TowerPartData TowerPart => _towerPart;
    public TowerType TowerType => _towerPart != null ? _towerPart.TowerType : TowerType.Basic;
    public event Action<TowerType> Clicked;

    public void Configure(
        TowerPartData towerPart,
        Button button,
        Image background,
        Image towerImage,
        TMP_Text nameText)
    {
        _towerPart = towerPart;
        _button = button;
        _background = background;
        _towerImage = towerImage;
        _nameText = nameText;
    }

    public void ConfigureTowerPart(TowerPartData towerPart)
    {
        _towerPart = towerPart;

        if (_towerImage != null)
        {
            _towerImage.sprite = towerPart != null ? towerPart.Sprite : null;
            _towerImage.enabled = towerPart != null && towerPart.Sprite != null;
        }

        if (_nameText != null)
            _nameText.text = towerPart != null ? towerPart.TowerName : string.Empty;
    }

    public void Initialize()
    {
        if (_initialized)
            return;

        _initialized = true;

        ConfigureTowerPart(_towerPart);

        _button.onClick.AddListener(() => Clicked?.Invoke(TowerType));
    }

    public void SetSelected(bool selected)
    {
        _background.color = selected ? SelectedBackground : IdleBackground;
        _nameText.color = selected ? SelectedText : Color.white;
    }
}
