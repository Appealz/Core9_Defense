using System;

public sealed class PreparationAutoSkipPresenter : IDisposable
{
    private readonly PreparationAutoSkipView _view;
    private readonly PreparationAutoSkipSetting _setting;
    private readonly GameStateManager _gameStateManager;

    public PreparationAutoSkipPresenter(
        PreparationAutoSkipView view,
        PreparationAutoSkipSetting setting,
        GameStateManager gameStateManager)
    {
        _view = view ?? throw new ArgumentNullException(nameof(view));
        _setting = setting ?? throw new ArgumentNullException(nameof(setting));
        _gameStateManager = gameStateManager ?? throw new ArgumentNullException(nameof(gameStateManager));

        _view.Initialize();
        _view.ValueChanged += HandleValueChanged;
        _setting.Changed += HandleSettingChanged;
        _gameStateManager.StateChanged += HandleStateChanged;
        Synchronize();
    }

    private void HandleValueChanged(bool isEnabled)
    {
        _setting.SetEnabled(isEnabled);
    }

    private void HandleSettingChanged(bool _)
    {
        Synchronize();
    }

    private void HandleStateChanged(GameState _, GameState __)
    {
        Synchronize();
    }

    public void Synchronize()
    {
        GameState state = _gameStateManager.CurrentState;
        bool isPlaying = state == GameState.Playing;
        _view.SetValue(_setting.IsEnabled);
        _view.SetBattleToggleState(
            state == GameState.Initializing || isPlaying,
            isPlaying);
    }

    public void Dispose()
    {
        _view.ValueChanged -= HandleValueChanged;
        _setting.Changed -= HandleSettingChanged;
        _gameStateManager.StateChanged -= HandleStateChanged;
    }
}
