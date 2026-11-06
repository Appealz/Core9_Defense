using TMPro;
using UnityEngine;

public sealed class ScoreHUDView : MonoBehaviour
{
    [SerializeField] private TMP_Text _scoreText;

    public bool IsConfigured => _scoreText != null;

    public void SetScore(int score)
    {
        _scoreText.text = $"Score {Mathf.Max(0, score):N0}";
    }
}
