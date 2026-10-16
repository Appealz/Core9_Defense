using System;

[Serializable]
public abstract class TowerAttackDefinition
{
    public abstract ITowerAttack CreateAttack(TowerAttackDependencies dependencies);

    public virtual ITowerAttackEffect CreateEffect()
    {
        return null;
    }
}