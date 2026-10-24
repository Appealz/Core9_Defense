
using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

[Serializable]
public class SplitOnDeathDefinition : EnemyDeathBehaviorDefinition
{
    [SerializeField] private EnemyConfig _childConfig;
    [SerializeField, Min(1)] private int _childCount = 2;
    [SerializeField, Min(0f)] private float _splitRadius = 0.75f;
    [SerializeField, Min(0f)] private float _splitDuration = 0.3f;

    public EnemyConfig ChildConfig => _childConfig;
    public int ChildCount => _childCount;
    public float SplitRadius => _splitRadius;
    public float SplitDuration => _splitDuration;

    public override void Execute(EnemyDeathContext context)
    {
        if (_childConfig == null || _childCount <= 0)
        {
            Debug.LogWarning($"분열 설정이 올바르지 않습니다: {context.SourceConfig.name}");
            return;
        }

        if (HasCycle(context.SourceConfig))
        {
            Debug.LogError($"Enemy 분열 순환 참조 발견: {context.SourceConfig.name}");
            return;
        }

        context.EnemyManager.ReserveAdditionalSpawns(_childCount);
        SpawnChildrenAsync(context, _childConfig, _childCount, _splitRadius, _splitDuration).Forget();
    }

    private static bool HasCycle(EnemyConfig sourceConfig)
    {
        HashSet<EnemyConfig> visited = new();
        EnemyConfig current = sourceConfig;

        while (current != null)
        {
            if (!visited.Add(current))
                return true;

            if (current.DeathBehaviorDefinition is not SplitOnDeathDefinition split)
                break;

            current = split.ChildConfig;
        }

        return false;
    }

    private static async UniTask SpawnChildrenAsync(EnemyDeathContext context, EnemyConfig childConfig, int count, float radius, float duration)
    {
        Vector3 toCore = context.SpawnManager.CorePosition - context.Position;
        toCore.z = 0f;

        Vector3 spreadDirection = new Vector3(-toCore.y, toCore.x, 0f).normalized;

        if (spreadDirection.sqrMagnitude < 0.0001f)
            spreadDirection = Vector3.right;

        for (int i = 0; i < count; i++)
        {
            Vector3 offset;

            if (count == 2)
            {
                offset = spreadDirection * (i == 0 ? -radius : radius);
            }
            else
            {
                float angle = Mathf.PI * 2f * i / count;
                offset = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * radius;
            }

            bool succeeded = false;

            try
            {
                await context.SpawnManager.SpawnEnemyAsync(childConfig, context.Position, context.Position + offset, duration);
                succeeded = true;
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
            finally
            {
                context.EnemyManager.CompleteReservedSpawn(succeeded);
            }
        }
    }
}
