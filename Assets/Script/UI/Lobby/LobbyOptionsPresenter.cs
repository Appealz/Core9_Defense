using System;

public sealed class LobbyOptionsPresenter : IDisposable
{
    private readonly LobbyOptionsView _view;
    private readonly UserAudioSettings _audioSettings;

    public LobbyOptionsPresenter(LobbyOptionsView view, UserAudioSettings audioSettings)
    {
        _view = view ?? throw new ArgumentNullException(nameof(view));
        _audioSettings = audioSettings ?? throw new ArgumentNullException(nameof(audioSettings));

        _view.Initialize();
        _view.CloseClicked += Hide;
        _view.BgmVolumeChanged += _audioSettings.SetBgmVolume;
        _view.SfxVolumeChanged += _audioSettings.SetSfxVolume;
        _view.SetVisible(false);
    }

    public void Show()
    {
        _view.Render(_audioSettings.BgmVolume, _audioSettings.SfxVolume);
        _view.SetVisible(true);
    }

    public void Hide() => _view.SetVisible(false);

    public bool TryClose()
    {
        if (!_view.IsVisible)
            return false;

        Hide();
        return true;
    }

    public void Dispose()
    {
        _view.CloseClicked -= Hide;
        _view.BgmVolumeChanged -= _audioSettings.SetBgmVolume;
        _view.SfxVolumeChanged -= _audioSettings.SetSfxVolume;
    }
}
