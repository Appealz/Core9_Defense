using System;
using UnityEngine;

[Serializable]
public abstract class TowerAttackDefinition
{
    public virtual ITowerAttack CreateAttack(ProjectileManager projectileManager)
    {
        throw new InvalidOperationException($"{GetType().Name}의 Attack 생성 방식이 구현되지 않았습니다.");
    }

    public virtual ITowerAttack CreateAttack(ProjectileManager projectileManager, ExplosionEffectManager explosionEffectManager)
    {
        return CreateAttack(projectileManager);
    }
}
