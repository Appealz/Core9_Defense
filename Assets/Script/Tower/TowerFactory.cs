using Cysharp.Threading.Tasks;
using System;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class TowerFactory
{
    private readonly TowerTargetSelector _targetSelector;
    private readonly TowerAttackDependencies _attackDependencies;
    private readonly TowerStatModifiers _globalStatModifiers;
    private readonly TowerStatModifiers _coreGoldStatModifiers;
    private readonly TowerTypeUpgradeState _typeUpgradeState;

    public TowerFactory(
        TowerTargetSelector targetSelector,
        TowerAttackDependencies attackDependencies,
        TowerStatModifiers globalStatModifiers,
        TowerStatModifiers coreGoldStatModifiers,
        TowerTypeUpgradeState typeUpgradeState)
    {
        _targetSelector = targetSelector ?? throw new ArgumentNullException(nameof(targetSelector));
        _attackDependencies = attackDependencies ?? throw new ArgumentNullException(nameof(attackDependencies));
        _globalStatModifiers = globalStatModifiers ?? throw new ArgumentNullException(nameof(globalStatModifiers));
        _coreGoldStatModifiers = coreGoldStatModifiers ?? throw new ArgumentNullException(nameof(coreGoldStatModifiers));
        _typeUpgradeState = typeUpgradeState ?? throw new ArgumentNullException(nameof(typeUpgradeState));
    }

    public async UniTask<Tower> CreateAsync(TowerPartData partData)
    {
        if (partData == null)
            throw new ArgumentNullException(nameof(partData));

        if (partData.AttackDefinition == null)
            throw new InvalidOperationException("AttackDefinition이 설정되지 않았습니다.");

        if (partData.FireModeDefinition == null)
            throw new InvalidOperationException("FireModeDefinition이 설정되지 않았습니다.");

        TowerStatModifiers typeStatModifiers = _typeUpgradeState.GetStatModifiers(partData.TowerType);
        TowerUniqueModifiers uniqueModifiers = _typeUpgradeState.GetUniqueModifiers(partData.TowerType);
        TowerStats stats = new TowerStats(
            partData,
            _globalStatModifiers,
            _coreGoldStatModifiers,
            typeStatModifiers);
        TowerHealth health = new TowerHealth(stats.MaxHp);

        ITowerAttack attack = partData.AttackDefinition.CreateAttack(_attackDependencies, uniqueModifiers);
        IFireMode fireMode = partData.FireModeDefinition.CreateFireMode(uniqueModifiers);

        TowerAttack towerAttack = new TowerAttack(stats, attack, fireMode, _targetSelector);

        GameObject towerObject = await Addressables.InstantiateAsync(AddressableKeys.TowerPrefab).Task;
        Tower tower = towerObject.GetComponent<Tower>();

        tower.gameObject.name = partData.TowerName;
        ITowerAttackEffect effect = partData.AttackDefinition.CreateEffect();
        tower.Initialize(
            health,
            stats,
            towerAttack,
            partData.Sprite,
            partData.TowerType,
            effect,
            partData.TowerName);

        return tower;
    }

    public void Release(Tower tower)
    {
        if (tower == null)
            return;

        Addressables.ReleaseInstance(tower.gameObject);
    }
}
