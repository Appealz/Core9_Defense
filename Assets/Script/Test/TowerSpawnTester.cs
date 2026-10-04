using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;

public class TowerSpawnTester : MonoBehaviour
{
    [SerializeField] private TowerPartData _basicTower;
    [SerializeField] private TowerPartData _rapidTower;
    [SerializeField] private TowerPartData _razerTower;
    [SerializeField] private TowerPartData _bombMissileTower;
    [SerializeField] private TowerPartData _areaTower;

    private TowerManager _towerManager;
    private bool _isSpawning;

    public void Initialize(TowerManager towerManager)
    {
        _towerManager = towerManager;
    }

    private void Update()
    {
        if (_towerManager == null || _isSpawning)
            return;

        Keyboard keyboard = Keyboard.current;

        if (keyboard == null)
            return;

        if (keyboard.qKey.wasPressedThisFrame)
            SpawnTowerAsync(_basicTower).Forget();
        else if (keyboard.wKey.wasPressedThisFrame)
            SpawnTowerAsync(_rapidTower).Forget();
        else if (keyboard.eKey.wasPressedThisFrame)
            SpawnTowerAsync(_razerTower).Forget();
        else if (keyboard.rKey.wasPressedThisFrame)
            SpawnTowerAsync(_bombMissileTower).Forget();
        else if (keyboard.tKey.wasPressedThisFrame)
            SpawnTowerAsync(_areaTower).Forget();
    }

    private async UniTask SpawnTowerAsync(TowerPartData partData)
    {
        if (partData == null)
            return;

        _isSpawning = true;

        bool isPlaced = await _towerManager.TryPlaceNextTowerAsync(partData);

        Debug.Log(isPlaced
            ? $"Tower 배치 성공: {partData.name}"
            : $"Tower 배치 실패: {partData.name}");

        _isSpawning = false;
    }
}