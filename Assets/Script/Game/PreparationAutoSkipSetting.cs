using System;

public sealed class PreparationAutoSkipSetting
{
    public event Action<bool> Changed;

    public bool IsEnabled { get; private set; }

    public void SetEnabled(bool isEnabled)
    {
        if (IsEnabled == isEnabled)
            return;

        IsEnabled = isEnabled;
        Changed?.Invoke(IsEnabled);
    }
}
