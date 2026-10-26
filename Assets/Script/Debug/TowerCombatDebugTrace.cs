#if UNITY_EDITOR
using UnityEngine;

public static class TowerCombatDebugTrace
{
    public static bool Enabled { get; set; }

    public static void LogAttack(string attackType, float damage, bool isCritical)
    {
        if (!Enabled)
            return;

        Debug.Log(
            $"[Combat Debug] Attack={attackType}, Critical={isCritical}, FinalDamage={damage:0.###}"
        );
    }

    public static void LogEnemyDamage(string enemyName, float damage, float hpBefore, float hpAfter)
    {
        if (!Enabled)
            return;

        Debug.Log(
            $"[Combat Debug] Enemy={enemyName}, AppliedDamage={damage:0.###}, " +
            $"HP={hpBefore:0.###} -> {hpAfter:0.###}"
        );
    }
}
#endif
