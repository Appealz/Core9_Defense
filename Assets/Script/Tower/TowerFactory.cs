using Cysharp.Threading.Tasks;
using System;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class TowerFactory
{
    public async UniTask<Tower> CreateAsync(TowerPartData partData)
    {
        if (partData == null)
            throw new ArgumentNullException(nameof(partData));

        if (partData.AttackDefinition == null)
            throw new InvalidOperationException("AttackDefinition이 설정되지 않았습니다.");

        TowerHealth health = new TowerHealth(partData.MaxHp);
        TowerStats stats = new TowerStats(partData.AttackDamage, partData.AttackRate, partData.AttackRange);
        ITowerAttack attack = partData.AttackDefinition.CreateAttack();
        TowerAttack towerAttack = new TowerAttack(stats, attack);

        GameObject towerObject = await Addressables.InstantiateAsync(AddressableKeys.TowerPrefab).Task;
        Tower tower = towerObject.GetComponent<Tower>();

        tower.gameObject.name = partData.TowerName;
        tower.Initialize(health, stats, towerAttack, partData.Sprite);

        return tower;
    }

    public void Release(Tower tower)
    {
        if (tower == null)
            return;

        Addressables.ReleaseInstance(tower.gameObject);
    }
}