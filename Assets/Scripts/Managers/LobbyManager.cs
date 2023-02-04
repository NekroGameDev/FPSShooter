using FishNet.Managing;
using FishNet.Transporting;
using FishNet.Transporting.Tugboat;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LobbyManager : MonoBehaviour
{
    [SerializeField] private GameObject menuCanvas;
    [SerializeField] private GameObject serverCanvas;

    [Space, SerializeField] private TextMeshProUGUI textServer;

    [Space, SerializeField] private NetworkManager networkManager;
    [SerializeField] private Tugboat tugboat;

    private string ipAddress;
    public string IpAddress
    {
        set
        {
            ipAddress = value;
            tugboat.SetClientAddress(ipAddress);
        }
    }

    private void Start()
    {
        tugboat.OnClientConnectionState += OnClientConnectionState;
        tugboat.OnServerConnectionState += OnServerConnectionState;
    }

    public void Host()
    {
        networkManager.ServerManager.StartConnection();
        networkManager.ClientManager.StartConnection();
    }

    public void Server()
    {
        networkManager.ServerManager.StartConnection();
    }

    public void CloseServer()
    {
        networkManager.ServerManager.StopConnection(true);
    }

    public void Client()
    {
        if (string.IsNullOrEmpty(ipAddress)) { IpAddress = "localhost"; }

        networkManager.ClientManager.StartConnection();
    }

    private void OnClientConnectionState(ClientConnectionStateArgs m_data)
    {        
        switch (m_data.ConnectionState)
        {
            case LocalConnectionState.Stopped:
                SceneManager.LoadScene(0);
                break;
            case LocalConnectionState.Started:
                SceneManager.LoadScene(1);
                break;
        }
    }

    private void OnServerConnectionState(ServerConnectionStateArgs m_data)
    {
        if (!networkManager.IsServerOnly) { return; }

        switch (m_data.ConnectionState)
        {
            case LocalConnectionState.Stopped:
                SceneManager.LoadScene(0);
                break;
            case LocalConnectionState.Started:
                SceneManager.LoadScene(1);
                break;
        }

        menuCanvas.SetActive(!networkManager.IsServer);
        serverCanvas.SetActive(networkManager.IsServer);

        if (networkManager.IsServer)
        {
            textServer.text = $"Server started";
        }
    }
}
