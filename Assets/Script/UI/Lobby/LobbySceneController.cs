using UnityEngine;

public sealed class LobbySceneController : MonoBehaviour
{
    [SerializeField] private LobbyView _lobbyView;
    [SerializeField] private UpgradeView _upgradeView;
    [SerializeField] private PersistentUpgradeCatalog _upgradeCatalog;

    private LobbyPresenter _lobbyPresenter;
    private UpgradePresenter _upgradePresenter;

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
            LobbyRuntimeViewFactory.Build(
                _upgradeCatalog,
                out _lobbyView,
                out _upgradeView);
        }

        var wallet = new PersistentCurrencyWallet();
        var upgradeService = new PersistentUpgradeService(_upgradeCatalog, wallet);
        _lobbyPresenter = new LobbyPresenter(_lobbyView);
        _upgradePresenter = new UpgradePresenter(
            _upgradeView,
            _upgradeCatalog,
            upgradeService,
            wallet);
        _lobbyPresenter.UpgradeRequested += OpenUpgrade;
        _upgradePresenter.BackRequested += OpenLobby;
        OpenLobby();
    }

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
