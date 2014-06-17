using JetBrains.Annotations;
using UnityEngine;

public class PlayButtonScript : MonoBehaviour 
{
    [UsedImplicitly]
	void Awake()
    {
        NotificationCenter.DefaultCenter.AddObserver(this, "OnHostListReceived");
    }

    [UsedImplicitly]
    void OnClick()
    {
        Debug.Log("PlayButtonScript OnClick");

        NetworkManager.Instance.RefreshHostList();
    }

    [UsedImplicitly]
    void OnHostListReceived()
    {
        Debug.Log("PlayButtonScript OnHostListReceived");

        NetworkManager.Instance.ConnectToGame();
    }

    [UsedImplicitly]
    void OnServerInitialized()
    {
        Debug.Log("PlayButtonScript OnServerInitialized");
    }

    [UsedImplicitly]
    void OnPlayerConnected()
    {
        Debug.Log("PlayerButtonScript OnPlayerConnected");

        Application.LoadLevel("NetworkedGameScene");
    }

    [UsedImplicitly]
    void OnConnectedToServer()
    {
        Debug.Log("PlayButtonScript OnConnectedToServer");

        Application.LoadLevel("NetworkedGameScene");
    }
}
