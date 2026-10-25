using System;

[Serializable]
public abstract class EnemyAbilityDefinition
{
    public virtual bool CanReceiveHealing => true;

    public abstract IEnemyAbility CreateAbility(Enemy owner);
}
