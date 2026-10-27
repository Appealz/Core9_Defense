
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WaveHUDView : MonoBehaviour
{
    [SerializeField] private Image _progressFill;
    [SerializeField] private TMP_Text _waveNumberText;
    [SerializeField] private TMP_Text _levelNumberText;    

    public void SetWaveNumber(int waveNumber)
    {
        _waveNumberText.text = $"WAVE {waveNumber}";
    }

    public void SetLevel(int level)
    {
        _levelNumberText.text = $"{level}";
    }

    public void SetExperience(int currentExperience, int requiredExperience)
    {
        _progressFill.fillAmount = requiredExperience > 0
            ? Mathf.Clamp01((float)currentExperience / requiredExperience)
            : 0f;
    }
}
