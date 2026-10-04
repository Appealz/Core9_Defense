using UnityEngine;

public class MeleeAttack : IEnemyAttack
{
    public void Attack(Tower target, float damage)
    {
        if (target == null || target.Health == null)
            return;

        float beforeHp = target.Health.CurrentHp;

        target.Health.TakeDamage(damage);

        Debug.Log(
            $"[Tower Hit] {target.name} / Damage: {damage} / HP: {beforeHp} ¡æ {target.Health.CurrentHp}");
    }
}