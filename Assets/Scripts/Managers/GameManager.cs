using FishNet;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Transporting.Tugboat;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    [SerializeField] private NetworkObject _playerPrefab;
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private GameObject _serverCamera;
    
    [SerializeField] private bool _addToDefaultScene = true;

    private NetworkManager _networkManager;

    public Transform[] SpawnPoints => spawnPoints;

    public static GameManager Instance;
    private void Start()
    {
        Instance = this;
        InitializeOnce();
    }

    private void OnDestroy()
    {
        if (_networkManager != null)
            _networkManager.SceneManager.OnClientLoadedStartScenes -= SceneManager_OnClientLoadedStartScenes;
    }


    /// <summary>
    /// Initializes this script for use.
    /// </summary>
    private void InitializeOnce()
    {
        _networkManager = InstanceFinder.NetworkManager;
        if (_networkManager == null)
        {
            Debug.LogWarning($"PlayerSpawner on {gameObject.name} cannot work as NetworkManager wasn't found on this object or within parent objects.");
            return;
        }

        _networkManager.SceneManager.OnClientLoadedStartScenes += SceneManager_OnClientLoadedStartScenes;
    }

    //public override void OnStartServer()
    //{
    //    base.OnStartServer();
    //    _networkManager.SceneManager.OnClientLoadedStartScenes += SceneManager_OnClientLoadedStartScenes;
    //}

    //public override void OnStartClient()
    //{
    //    base.OnStartClient();
    //    SpawnPlayer();
    //}

    //[ServerRpc(RequireOwnership = false)]
    //private void SpawnPlayer(NetworkConnection client = null)
    //{
    //    GameObject m_newPlayer = Instantiate(_playerPrefab.gameObject, spawnPoints[Random.Range(0, spawnPoints.Length)].position, Quaternion.identity);
    //    Spawn(m_newPlayer, client);
    //}

    private void SceneManager_OnClientLoadedStartScenes(NetworkConnection conn, bool asServer)
    {
        if (!asServer)
            return;
        if (_playerPrefab == null)
        {
            Debug.LogWarning($"Player prefab is empty and cannot be spawned for connection {conn.ClientId}.");
            return;
        }

        GameObject m_newPlayer = Instantiate(_playerPrefab.gameObject, spawnPoints[Random.Range(0, spawnPoints.Length)].position, Quaternion.identity);
        _networkManager.ServerManager.Spawn(m_newPlayer, conn);
    }
}
