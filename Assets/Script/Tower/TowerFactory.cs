using Cysharp.Threading.Tasks;
using System;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class TowerFactory
{
    private readonly TowerTargetSelector _targetSelector;
    private readonly TowerAttackDependencies _attackDependencies;
    private readonly TowerStatModifiers _globalStatModifiers;

    public TowerFactory(TowerTargetSelector targetSelector, TowerAttackDependencies attackDependencies, TowerStatModifiers globalStatModifiers)
    {
        _targetSelector = targetSelector ?? throw new ArgumentNullException(nameof(targetSelector));
        _attackDependencies = attackDependencies ?? throw new ArgumentNullException(nameof(attackDependencies));
        _globalStatModifiers = globalStatModifiers ?? throw new ArgumentNullException(nameof(globalStatModifiers));
    }

    public async UniTask<Tower> CreateAsync(TowerPartData partData)
    {
        if (partData == null)
            throw new ArgumentNullException(nameof(partData));

        if (partData.AttackDefinition == null)
            throw new InvalidOperationException("AttackDefinition이 설정되지 않았습니다.");

        if (partData.FireModeDefinition == null)
            throw new InvalidOperationException("FireModeDefinition이 설정되지 않았습니다.");

        TowerStats stats = new TowerStats(partData, _globalStatModifiers);
        TowerHealth health = new TowerHealth(stats.MaxHp);

        ITowerAttack attack = partData.AttackDefinition.CreateAttack(_attackDependencies);
        IFireMode fireMode = partData.FireModeDefinition.CreateFireMode();

        TowerAttack towerAttack = new TowerAttack(stats, attack, fireMode, _targetSelector);

        GameObject towerObject = await Addressables.InstantiateAsync(AddressableKeys.TowerPrefab).Task;
        Tower tower = towerObject.GetComponent<Tower>();

        tower.gameObject.name = partData.TowerName;
        tower.Initialize(health, stats, towerAttack, partData.Sprite);

        ITowerAttackEffect effect = partData.AttackDefinition.CreateEffect();
        effect?.Show(tower.transform, stats);

        return tower;
    }

    public void Release(Tower tower)
    {
        if (tower == null)
            return;

        Addressables.ReleaseInstance(tower.gameObject);
    }
}
