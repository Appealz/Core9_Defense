using System;

public sealed class ScoreHUDPresenter : IDisposable
{
    private readonly ScoreHUDView _view;
    private readonly RunScoreCounter _scoreCounter;

    public ScoreHUDPresenter(ScoreHUDView view, RunScoreCounter scoreCounter)
    {
        _view = view ?? throw new ArgumentNullException(nameof(view));
        _scoreCounter = scoreCounter ?? throw new ArgumentNullException(nameof(scoreCounter));

        _scoreCounter.ScoreChanged += HandleScoreChanged;
        _view.SetScore(_scoreCounter.Score);
    }

    private void HandleScoreChanged(int score)
    {
        _view.SetScore(score);
    }

    public void Dispose()
    {
        _scoreCounter.ScoreChanged -= HandleScoreChanged;
    }
}
