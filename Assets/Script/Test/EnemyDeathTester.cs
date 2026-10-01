using UnityEngine;
using UnityEngine.InputSystem;

public class EnemyDeathTester : MonoBehaviour
{
    private void Update()
    {
        if (Keyboard.current == null || !Keyboard.current.kKey.wasPressedThisFrame)
            return;

        Enemy[] enemies = FindObjectsByType<Enemy>(FindObjectsInactive.Include);

        int killedCount = 0;
        int returnedCount = 0;

        foreach (Enemy enemy in enemies)
        {
            if (!enemy.gameObject.activeInHierarchy)
                continue;

            if (enemy.Health == null || !enemy.Health.IsAlive)
                continue;

            enemy.Health.TakeDamage(enemy.Health.CurrentHp);
            killedCount++;

            if (!enemy.gameObject.activeSelf)
                returnedCount++;
        }

        Debug.Log($"[Enemy Pool Test] Kill: {killedCount} / Return: {returnedCount}");

        if (killedCount == returnedCount)
            Debug.Log("[Enemy Pool Test] 모든 Enemy가 Pool로 정상 반환되었습니다.");
        else
            Debug.LogWarning("[Enemy Pool Test] Pool로 반환되지 않은 Enemy가 있습니다.");
    }
}