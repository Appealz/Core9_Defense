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
    [SerializeField] private TowerCatalog _towerCatalog;
    [SerializeField] private SceneTransition _sceneTransition;
    [SerializeField] private LobbyOptionsView _optionsView;
    [SerializeField] private ExitConfirmationView _exitConfirmationView;
    [SerializeField] private LobbyBackInput _backInput;

    private LobbyPresenter _lobbyPresenter;
    private UpgradePresenter _upgradePresenter;
    private LobbyOptionsPresenter _optionsPresenter;
    private LobbyExitPresenter _exitPresenter;
    private PersistentCurrencyWallet _wallet;
    private PersistentUpgradeService _upgradeService;

    public void Configure(
        LobbyView lobbyView,
        UpgradeView upgradeView,
        PersistentUpgradeCatalog upgradeCatalog,
        TowerCatalog towerCatalog = null,
        SceneTransition sceneTransition = null,
        LobbyOptionsView optionsView = null,
        ExitConfirmationView exitConfirmationView = null,
        LobbyBackInput backInput = null)
    {
        _lobbyView = lobbyView;
        _upgradeView = upgradeView;
        _upgradeCatalog = upgradeCatalog;
        _towerCatalog = towerCatalog;
        _sceneTransition = sceneTransition;
        _optionsView = optionsView;
        _exitConfirmationView = exitConfirmationView;
        _backInput = backInput;
    }

    private void Awake()
    {
        if (_upgradeCatalog == null || _towerCatalog == null)
        {
            Debug.LogError("Lobby tower or persistent upgrade catalog is missing.", this);
            enabled = false;
            return;
        }

        if (SceneTransition.Instance != null)
            _sceneTransition = SceneTransition.Instance;

        if (_lobbyView == null || _upgradeView == null || _sceneTransition == null ||
            _optionsView == null || !_optionsView.IsConfigured ||
            _exitConfirmationView == null || !_exitConfirmationView.IsConfigured || _backInput == null)
        {
            Debug.LogError(
                "Lobby UI, options, exit confirmation, back input, and SceneTransition must be assigned from the Lobby scene.",
                this);
            enabled = false;
            return;
        }

        _wallet = new PersistentCurrencyWallet();
        _upgradeService = new PersistentUpgradeService(
            _upgradeCatalog,
            _wallet,
            towerCatalog: _towerCatalog);
        _lobbyPresenter = new LobbyPresenter(_lobbyView, _sceneTransition);
        _optionsPresenter = new LobbyOptionsPresenter(
            _optionsView,
            new UserAudioSettings());
        _exitPresenter = new LobbyExitPresenter(
            _exitConfirmationView,
            _backInput,
            _sceneTransition,
            TryCloseTopmostLobbyContent);
        _upgradePresenter = new UpgradePresenter(
            _upgradeView,
            _upgradeCatalog,
            _towerCatalog,
            _upgradeService,
            _wallet);
        _lobbyPresenter.UpgradeRequested += OpenUpgrade;
        _lobbyPresenter.OptionsRequested += OpenOptions;
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
        _optionsPresenter.Hide();
        _lobbyPresenter.Hide();
        _upgradePresenter.Show();
    }

    private void OpenOptions()
    {
        _optionsPresenter.Show();
    }

    private bool TryCloseTopmostLobbyContent()
    {
        if (_optionsPresenter != null && _optionsPresenter.TryClose())
            return true;

        if (_upgradePresenter == null || !_upgradePresenter.IsVisible)
            return false;

        OpenLobby();
        return true;
    }

    private void OnDestroy()
    {
        if (_lobbyPresenter != null)
        {
            _lobbyPresenter.UpgradeRequested -= OpenUpgrade;
            _lobbyPresenter.OptionsRequested -= OpenOptions;
        }

        if (_upgradePresenter != null)
            _upgradePresenter.BackRequested -= OpenLobby;

        _upgradePresenter?.Dispose();
        _optionsPresenter?.Dispose();
        _exitPresenter?.Dispose();
        _lobbyPresenter?.Dispose();
    }
}
