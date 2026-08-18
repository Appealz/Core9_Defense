using UnityEngine;
using System;

public class TowerFactory
{
    public Tower Create(TowerPartData partData)
    {
        if (partData == null)
            throw new ArgumentNullException(nameof(partData));

        if (partData.AttackDefinition == null)
            throw new InvalidOperationException("TowerPartData의 AttackDefinition이 설정되지 않았습니다.");

        TowerHealth health = new TowerHealth(partData.MaxHp);

        TowerStats stats = new TowerStats(partData.AttackDamage, partData.AttackRate, partData.AttackRange);

        ITowerAttack attack = partData.AttackDefinition.CreateAttack();

        TowerAttack towerAttack = new TowerAttack(stats, attack);

        return new Tower(health, stats, towerAttack);
    }
}
