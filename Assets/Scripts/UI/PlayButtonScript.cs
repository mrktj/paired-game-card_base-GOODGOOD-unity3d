using System.Linq;
using JetBrains.Annotations;
using UnityEngine;

public class PlayButtonScript : MonoBehaviour
{
    public int Port = 8672;
    public string GameType = "PairedGame_Development";

    [UsedImplicitly]
    private void OnClick()
    {
        Debug.Log("PlayButtonScript OnClick");

        MasterServer.ClearHostList();
        MasterServer.RequestHostList(GameType);
    }

    private void ConnectToHost()
    {
        var hostList = MasterServer.PollHostList();
        var host = hostList.FirstOrDefault(h => h.connectedPlayers < h.playerLimit);

        Debug.Log("PlayButtonScript ConnectToHost: " + hostList.Length + " games available");

        if (host == null)
        {
            Network.InitializeServer(2, Port, !Network.HavePublicAddress());
            MasterServer.RegisterHost(GameType, "PairedGame");
        }
        else
        {
            Network.Connect(host);
        }
    }

    [UsedImplicitly]
    private void OnMasterServerEvent(MasterServerEvent msEvent)
    {
        if (msEvent == MasterServerEvent.HostListReceived)
        {
            Debug.Log("PlayButtonScript OnMasterServerEvent: HostListRecieved");

            ConnectToHost();
        }
    }

    [UsedImplicitly]
    private void OnServerInitialized()
    {
        Debug.Log("PlayButtonScript OnServerInitialized");
    }

    [UsedImplicitly]
    private void OnPlayerConnected()
    {
        Debug.Log("PlayerButtonScript OnPlayerConnected");

        Application.LoadLevel("NetworkedGameScene");
    }

    [UsedImplicitly]
    private void OnConnectedToServer()
    {
        Debug.Log("PlayButtonScript OnConnectedToServer");

        Application.LoadLevel("NetworkedGameScene");
    }
}