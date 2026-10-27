using System;

[Serializable]
public sealed class SingleFireModeDefinition : FireModeDefinition
{
    public override IFireMode CreateFireMode(TowerUniqueModifiers uniqueModifiers)
    {
        return new SingleFireMode();
    }
}
