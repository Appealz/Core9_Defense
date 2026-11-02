using UnityEngine;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
using UnityEngine.InputSystem;
#endif

public sealed class LobbySceneController : MonoBehaviour
{
    private const int TestCurrencyGrant = 5_000;

    [SerializeField] private LobbyView _lobbyView;
    [SerializeField] private UpgradeView _upgradeView;
    [SerializeField] private PersistentUpgradeCatalog _upgradeCatalog;

    private LobbyPresenter _lobbyPresenter;
    private UpgradePresenter _upgradePresenter;
    private PersistentCurrencyWallet _wallet;
    private PersistentUpgradeService _upgradeService;

    public void Configure(
        LobbyView lobbyView,
        UpgradeView upgradeView,
        PersistentUpgradeCatalog upgradeCatalog)
    {
        _lobbyView = lobbyView;
        _upgradeView = upgradeView;
        _upgradeCatalog = upgradeCatalog;
    }

    private void Awake()
    {
        if (_upgradeCatalog == null)
        {
            Debug.LogError("Lobby upgrade catalog is missing.", this);
            enabled = false;
            return;
        }

        if (_lobbyView == null || _upgradeView == null)
        {
            Debug.LogError(
                "LobbyView and UpgradeView must be assigned from the Lobby scene.",
                this);
            enabled = false;
            return;
        }

        _wallet = new PersistentCurrencyWallet();
        _upgradeService = new PersistentUpgradeService(_upgradeCatalog, _wallet);
        _lobbyPresenter = new LobbyPresenter(_lobbyView);
        _upgradePresenter = new UpgradePresenter(
            _upgradeView,
            _upgradeCatalog,
            _upgradeService,
            _wallet);
        _lobbyPresenter.UpgradeRequested += OpenUpgrade;
        _upgradePresenter.BackRequested += OpenLobby;
        OpenLobby();
    }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
    private void Update()
    {
        Keyboard keyboard = Keyboard.current;

        if (keyboard == null)
            return;

        if (keyboard.gKey.wasPressedThisFrame && _wallet.Add(TestCurrencyGrant))
        {
            Debug.Log(
                $"[Persistent Upgrade Test] Granted {TestCurrencyGrant:N0} currency. " +
                $"Balance: {_wallet.Balance:N0}",
                this);
        }

        if (keyboard.rKey.wasPressedThisFrame)
        {
            _wallet.Reset();
            _upgradeService.ResetAllLevels();
            Debug.Log(
                "[Persistent Upgrade Test] Currency and all CORE/TOWER upgrade levels reset.",
                this);
        }
    }
#endif

    private void OpenLobby()
    {
        _upgradePresenter.Hide();
        _lobbyPresenter.Show();
    }

    private void OpenUpgrade()
    {
        _lobbyPresenter.Hide();
        _upgradePresenter.Show();
    }

    private void OnDestroy()
    {
        if (_lobbyPresenter != null)
            _lobbyPresenter.UpgradeRequested -= OpenUpgrade;

        if (_upgradePresenter != null)
            _upgradePresenter.BackRequested -= OpenLobby;

        _upgradePresenter?.Dispose();
        _lobbyPresenter?.Dispose();
    }
}
