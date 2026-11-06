using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class SceneTransition : MonoBehaviour
{
    [SerializeField, Min(0f)] private float _fadeOutDuration = 0.35f;
    [SerializeField, Min(0f)] private float _fadeInDuration = 0.5f;
    [SerializeField] private Color _fadeColor = Color.black;
    [SerializeField] private CanvasGroup _canvasGroup;
    [SerializeField] private Image _fadeImage;

    private bool _isTransitioning;

    public static SceneTransition Instance { get; private set; }
    public bool IsTransitioning => _isTransitioning;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStaticState()
    {
        Instance = null;
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        if (_canvasGroup == null || _fadeImage == null)
        {
            Debug.LogError("SceneTransition requires a CanvasGroup and Fade Image.", this);
            enabled = false;
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        ApplyFadeColor();
        _canvasGroup.alpha = 0f;
        SetInputBlocked(false);
    }

    private void OnValidate()
    {
        _fadeOutDuration = Mathf.Max(0f, _fadeOutDuration);
        _fadeInDuration = Mathf.Max(0f, _fadeInDuration);
        ApplyFadeColor();
    }

    public bool TryLoadScene(string sceneName)
    {
        if (!enabled || _isTransitioning || string.IsNullOrWhiteSpace(sceneName))
            return false;

        _isTransitioning = true;
        SetInputBlocked(true);
        LoadSceneAsync(sceneName).Forget();
        return true;
    }

    public async UniTask FadeInAsync()
    {
        if (!_isTransitioning)
            return;

        await FadeToAsync(0f, _fadeInDuration);
        SetInputBlocked(false);
        _isTransitioning = false;
    }

    private async UniTask LoadSceneAsync(string sceneName)
    {
        CancellationToken destroyCancellationToken = this.GetCancellationTokenOnDestroy();

        try
        {
            await FadeToAsync(1f, _fadeOutDuration);

            AsyncOperation loadOperation = SceneManager.LoadSceneAsync(
                sceneName,
                LoadSceneMode.Single);

            if (loadOperation == null)
                throw new InvalidOperationException($"Could not load scene '{sceneName}'.");

            await loadOperation.ToUniTask(cancellationToken: destroyCancellationToken);
        }
        catch (OperationCanceledException) when (destroyCancellationToken.IsCancellationRequested)
        {
            return;
        }
        catch (Exception exception)
        {
            Debug.LogException(exception, this);

            if (this == null || Instance != this)
                return;

            await FadeToAsync(0f, _fadeInDuration);
            SetInputBlocked(false);
            _isTransitioning = false;
        }
    }

    private async UniTask FadeToAsync(float targetAlpha, float duration)
    {
        float startAlpha = _canvasGroup.alpha;

        if (duration <= 0f || Mathf.Approximately(startAlpha, targetAlpha))
        {
            _canvasGroup.alpha = targetAlpha;
            return;
        }

        float elapsed = 0f;

        while (elapsed < duration)
        {
            await UniTask.Yield(PlayerLoopTiming.Update, this.GetCancellationTokenOnDestroy());
            elapsed += Time.unscaledDeltaTime;
            float progress = Mathf.Clamp01(elapsed / duration);
            _canvasGroup.alpha = Mathf.SmoothStep(startAlpha, targetAlpha, progress);
        }

        _canvasGroup.alpha = targetAlpha;
    }

    private void SetInputBlocked(bool blocked)
    {
        if (_canvasGroup == null)
            return;

        _canvasGroup.interactable = blocked;
        _canvasGroup.blocksRaycasts = blocked;
    }

    private void ApplyFadeColor()
    {
        if (_fadeImage == null)
            return;

        _fadeImage.color = _fadeColor;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}
