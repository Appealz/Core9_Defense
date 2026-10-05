using Cysharp.Threading.Tasks;
using System;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class TowerFactory
{
    private readonly TowerTargetSelector _targetSelector;
    private readonly ProjectileManager _projectileManager;

    public TowerFactory(TowerTargetSelector targetSelector, ProjectileManager projectileManager)
    {
        _targetSelector = targetSelector ?? throw new ArgumentNullException(nameof(targetSelector));
        _projectileManager = projectileManager ?? throw new ArgumentNullException(nameof(projectileManager));
    }

    public async UniTask<Tower> CreateAsync(TowerPartData partData)
    {
        if (partData == null)
            throw new ArgumentNullException(nameof(partData));

        if (partData.AttackDefinition == null)
            throw new InvalidOperationException("AttackDefinition이 설정되지 않았습니다.");

        if (partData.FireModeDefinition == null)
            throw new InvalidOperationException("FireModeDefinition이 설정되지 않았습니다.");

        TowerHealth health = new TowerHealth(partData.MaxHp);
        TowerStats stats = new TowerStats(partData.AttackDamage, partData.AttackRate, partData.AttackRange);

        ITowerAttack attack = partData.AttackDefinition.CreateAttack(_projectileManager);
        IFireMode fireMode = partData.FireModeDefinition.CreateFireMode();

        TowerAttack towerAttack = new TowerAttack(stats, attack, fireMode, _targetSelector);

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