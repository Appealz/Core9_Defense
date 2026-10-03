using UnityEngine;

public class MeleeAttack : IEnemyAttack
{
    public void Attack(Tower target, float damage)
    {
        if (target == null || target.Health == null)
            return;

        float beforeHp = target.Health.CurrentHp;

        target.Health.TakeDamage(damage);

        Debug.Log($"[MeleeAttack] Damage: {damage} / HP: {beforeHp} -> {target.Health.CurrentHp}");
    }
}
