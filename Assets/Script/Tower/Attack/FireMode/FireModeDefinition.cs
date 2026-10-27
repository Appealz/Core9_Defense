using System;

[Serializable]
public abstract class FireModeDefinition
{
    public abstract IFireMode CreateFireMode(TowerUniqueModifiers uniqueModifiers);
}
