using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

public sealed class FlyingGemService : IUpdatable, IDisposable
{
    private readonly FlyingGem _prefab;
    private readonly Transform _poolRoot;
    private readonly FlyingGemSettings _settings;
    private readonly GameStateManager _gameStateManager;
    private readonly Camera _worldCamera;
    private readonly CombatBoundsProvider _combatBoundsProvider;
    private readonly CombatTextManager _combatTextManager;
    private readonly IGemsWallet _wallet;
    private readonly ObjectPool<FlyingGem> _pool;

    private FlyingGem _activeGem;
    private float _spawnTimer;
    private bool _isSpawning;
    private bool _isDisposed;

    public bool HasActiveGem => _activeGem != null;

    public FlyingGemService(
        FlyingGem prefab,
        Transform poolRoot,
        FlyingGemSettings settings,
        GameStateManager gameStateManager,
        Camera worldCamera,
        CombatBoundsProvider combatBoundsProvider,
        CombatTextManager combatTextManager,
        IGemsWallet wallet)
    {
        _prefab = prefab != null ? prefab : throw new ArgumentNullException(nameof(prefab));
        _poolRoot = poolRoot != null ? poolRoot : throw new ArgumentNullException(nameof(poolRoot));
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        _gameStateManager = gameStateManager ?? throw new ArgumentNullException(nameof(gameStateManager));
        _worldCamera = worldCamera != null ? worldCamera : throw new ArgumentNullException(nameof(worldCamera));
        _combatBoundsProvider = combatBoundsProvider ?? throw new ArgumentNullException(nameof(combatBoundsProvider));
        _combatTextManager = combatTextManager ?? throw new ArgumentNullException(nameof(combatTextManager));
        _wallet = wallet ?? throw new ArgumentNullException(nameof(wallet));

        _settings.Validate();
        _pool = new ObjectPool<FlyingGem>(
            CreateInstanceAsync,
            onReturn: gem =>
            {
                gem.ResetForPool();
                gem.gameObject.SetActive(false);
            });
        _gameStateManager.StateChanged += HandleStateChanged;
    }

    public async UniTask InitializeAsync()
    {
        await _pool.PrewarmAsync(_settings.MaxActiveGems);
        ScheduleNextSpawn();
    }

    public void Update(float deltaTime)
    {
        if (_isDisposed || _gameStateManager.CurrentState != GameState.Playing)
            return;

        float unscaledDeltaTime = Mathf.Max(0f, Time.unscaledDeltaTime);

        if (_activeGem != null)
        {
            _activeGem.SetMovementBounds(CreateMovementBounds());

            if (!_activeGem.Tick(unscaledDeltaTime))
                ReleaseActiveGem(scheduleNextSpawn: true);

            return;
        }

        if (_isSpawning)
            return;

        _spawnTimer -= unscaledDeltaTime;

        if (_spawnTimer <= 0f)
            SpawnAsync().Forget();
    }

    public bool TryCollect(Vector2 screenPosition)
    {
        if (_isDisposed || _gameStateManager.CurrentState != GameState.Playing || _activeGem == null)
            return false;

        Vector3 world = _worldCamera.ScreenToWorldPoint(
            new Vector3(screenPosition.x, screenPosition.y, Mathf.Abs(_worldCamera.transform.position.z)));

        if (!_activeGem.TryBeginCollection(world))
            return false;

        Vector3 collectionPosition = _activeGem.WorldPosition;
        int reward = UnityEngine.Random.Range(_settings.RewardMin, _settings.RewardMax + 1);
        _wallet.Add(reward);
        _combatTextManager.ShowGemReward(collectionPosition, reward);
        ReleaseActiveGem(scheduleNextSpawn: true);
        return true;
    }

    private async UniTaskVoid SpawnAsync()
    {
        _isSpawning = true;

        try
        {
            FlyingGem gem = await _pool.GetAsync();

            if (_isDisposed || _activeGem != null || _gameStateManager.CurrentState != GameState.Playing)
            {
                _pool.Return(gem);
                return;
            }

            CombatBounds bounds = CreateMovementBounds();
            Vector2 position = new(
                UnityEngine.Random.Range(bounds.Left, bounds.Right),
                UnityEngine.Random.Range(bounds.Bottom, bounds.Top));
            Vector2 direction = UnityEngine.Random.insideUnitCircle;

            if (direction.sqrMagnitude <= 0.0001f)
                direction = Vector2.right;

            float lifetime = UnityEngine.Random.Range(_settings.LifetimeMin, _settings.LifetimeMax);
            gem.Initialize(
                position,
                direction,
                _settings.MoveSpeed,
                lifetime,
                bounds,
                _settings.VisualScale,
                _settings.TouchHitboxScale,
                _settings.BounceRandomAngle,
                _settings.EnableTouchHint,
                _settings.TouchHintInterval,
                _settings.TouchHintDuration,
                _settings.TouchHintBlinkCount);
            _activeGem = gem;
        }
        finally
        {
            _isSpawning = false;
        }
    }

    private CombatBounds CreateMovementBounds()
    {
        CombatBounds source = _combatBoundsProvider.GetBounds();
        float visualExtent = 0.5f * _settings.VisualScale;
        float left = source.Left + _settings.LeftMargin + visualExtent;
        float right = source.Right - _settings.RightMargin - visualExtent;
        float bottom = source.Bottom + _settings.BottomMargin + visualExtent;
        float top = source.Top - _settings.TopMargin - visualExtent;

        if (left >= right)
        {
            float center = source.Center.x;
            left = center - 0.05f;
            right = center + 0.05f;
        }

        if (bottom >= top)
        {
            float center = source.Center.y;
            bottom = center - 0.05f;
            top = center + 0.05f;
        }

        return new CombatBounds(left, right, bottom, top);
    }

    private void HandleStateChanged(GameState previousState, GameState nextState)
    {
        _activeGem?.SetTouchHintPlaying(nextState == GameState.Playing);

        if (nextState == GameState.GameOver || nextState == GameState.GameResult)
            ReleaseActiveGem(scheduleNextSpawn: false);
    }

    private void ReleaseActiveGem(bool scheduleNextSpawn)
    {
        if (_activeGem != null)
        {
            FlyingGem gem = _activeGem;
            _activeGem = null;
            _pool.Return(gem);
        }

        if (scheduleNextSpawn)
            ScheduleNextSpawn();
    }

    private void ScheduleNextSpawn()
    {
        _spawnTimer = UnityEngine.Random.Range(
            _settings.SpawnIntervalMin,
            _settings.SpawnIntervalMax);
    }

    private UniTask<FlyingGem> CreateInstanceAsync()
    {
        FlyingGem instance = UnityEngine.Object.Instantiate(_prefab, _poolRoot, false);

        if (!instance.IsConfigured)
        {
            UnityEngine.Object.Destroy(instance.gameObject);
            throw new InvalidOperationException("FlyingGem prefab references are not configured.");
        }

        return UniTask.FromResult(instance);
    }

    public void Dispose()
    {
        if (_isDisposed)
            return;

        _isDisposed = true;
        _gameStateManager.StateChanged -= HandleStateChanged;
        ReleaseActiveGem(scheduleNextSpawn: false);
    }
}
