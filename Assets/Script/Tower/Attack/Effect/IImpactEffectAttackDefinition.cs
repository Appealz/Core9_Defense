using UnityEngine;

public interface IImpactEffectAttackDefinition
{
    ITowerAttack CreateAttack(ProjectileManager projectileManager, IImpactEffect impactEffect);
}
