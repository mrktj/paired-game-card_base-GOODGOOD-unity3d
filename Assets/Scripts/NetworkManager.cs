using System.Linq;
using JetBrains.Annotations;
using UnityEngine;

public class NetworkManager : MonoBehaviour
{
    public static int Port = 8672;
    public static string AppVersion = "0.1";
    public static string GameType = "PairedGameDevelopment_v" + AppVersion;

    [UsedImplicitly]
    private void Awake()
    {
        var networkManager = FindObjectOfType<NetworkManager>();

        if (networkManager != null)
        {
            DontDestroyOnLoad(transform.gameObject);
        }
        else
        {
            Destroy(transform.gameObject);

            Debug.Log("NetworkManager Awake: Destroying network manager");
        }
    }

    public static void ConnectToGame()
    {
        var hostList = MasterServer.PollHostList();

        Debug.Log("NetworkManager ConnectToGame: " + hostList.Length + " games available");

        foreach (var host in hostList.Where(host => host.connectedPlayers < host.playerLimit))
        {
            Network.Connect(host);
            return;
        }

        Network.InitializeServer(2, Port, !Network.HavePublicAddress());
        MasterServer.RegisterHost(GameType, "NetworkedGame", AppVersion);
    }

   public static void RefreshHostList()
    {
        Debug.Log("NetworkManager RefreshHostList");

        MasterServer.ClearHostList();
        MasterServer.RequestHostList(GameType);
    }
       
    [UsedImplicitly]
    void OnMasterServerEvent(MasterServerEvent msEvent)
    {
        if (msEvent == MasterServerEvent.HostListReceived)
        {
            Debug.Log("NetworkManager OnMasterServerEvent: HostListRecieved");

            NotificationCenter.DefaultCenter.PostNotification(this, "OnHostListReceived");
        }
    }

    [UsedImplicitly]
    void OnFailedToConnectToMasterServer(NetworkConnectionError info) 
    {
        Debug.Log("Could not connect to master server: " + info);
    }
}
