public sealed class GameEndRunState
{
    public bool ContinueUsed { get; private set; }

    public bool TryMarkContinueUsed()
    {
        if (ContinueUsed)
            return false;

        ContinueUsed = true;
        return true;
    }
}
