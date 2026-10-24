
using System;

[Serializable]
public abstract class EnemyDeathBehaviorDefinition
{
    public abstract void Execute(EnemyDeathContext context);
}
