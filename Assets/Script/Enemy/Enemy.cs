using UnityEngine;

public class Enemy : MonoBehaviour
{
    public EnemyHealth Health { get; private set; }
    public EnemyStats Stats { get; private set; }
    public IEnemyAttack Attack { get; private set; }

    private EnemyController _controller;
    private Transform _moveTarget;

    public void Initialize(EnemyHealth health, EnemyStats stats, EnemyController controller, IEnemyAttack attack, Transform moveTarget)
    {
        Health = health;
        Stats = stats;
        Attack = attack;
        _controller = controller;
        _moveTarget = moveTarget;
    }

    private void Update()
    {
        if (_controller == null || _moveTarget == null)
            return;

        _controller.EnemyUpdate(transform, _moveTarget.position, Time.deltaTime);
    }
}