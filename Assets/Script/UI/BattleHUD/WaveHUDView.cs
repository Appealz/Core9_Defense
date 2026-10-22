
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WaveHUDView : MonoBehaviour
{
    [SerializeField] private Image _progressFill;
    [SerializeField] private TMP_Text _waveNumberText;

    public void SetWaveNumber(int waveNumber)
    {
        _waveNumberText.text = waveNumber.ToString();
    }

    public void SetProgress(float progress)
    {
        _progressFill.fillAmount = Mathf.Clamp01(progress);
    }

    public void SetProgress(int processedCount, int totalCount)
    {
        float progress = totalCount > 0
            ? (float)processedCount / totalCount
            : 0f;

        SetProgress(progress);
    }
}
