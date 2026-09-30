using UnityEngine;

public class SpawnPositionTester : MonoBehaviour
{
    [SerializeField] private Camera _camera;
    [SerializeField] private CoreLayout _coreLayout;
    [SerializeField] private float _spawnMargin = 1f;

    private CameraBoundsProvider _boundsProvider;
    private SpawnPositionCalculator _calculator;

    private void Awake()
    {
        _boundsProvider = new CameraBoundsProvider(_camera);
        _calculator = new SpawnPositionCalculator();
    }

    private void OnDrawGizmos()
    {
        if (_camera == null || _coreLayout == null)
            return;

        var boundsProvider = new CameraBoundsProvider(_camera);
        var calculator = new SpawnPositionCalculator();

        CameraBounds bounds = boundsProvider.GetBounds();
        Vector2 origin = _coreLayout.transform.position;

        Gizmos.DrawWireSphere(origin, 0.15f);

        const int count = 32;

        for (int i = 0; i < count; i++)
        {
            float angle =
                Mathf.PI * 2f * i / count;

            Vector2 direction = new(
                Mathf.Cos(angle),
                Mathf.Sin(angle));

            Vector2 position = calculator.Calculate(
                bounds,
                origin,
                direction,
                _spawnMargin);

            Gizmos.DrawSphere(position, 0.15f);
            Gizmos.DrawLine(origin, position);
        }
    }
}