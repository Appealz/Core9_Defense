using System;

[Serializable]
public sealed class SingleFireModeDefinition : FireModeDefinition
{
    public override IFireMode CreateFireMode()
    {
        return new SingleFireMode();
    }
}