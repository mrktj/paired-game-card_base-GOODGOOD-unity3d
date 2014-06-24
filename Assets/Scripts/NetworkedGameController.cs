using System.Collections;
using JetBrains.Annotations;
using UnityEngine;

public class NetworkedGameController : AbstractGameController
{
    [UsedImplicitly]
    private new void Start()
    {
        base.Start();

        if (Network.isServer)
        {
            Debug.Log("NetworkedGameController Start: Generating Answer Key");

            GenerateAnswerKey();
        }
        else
        {
            Debug.Log("NetworkedGameController Start: Waiting for Answer Key...");
        }
    }

    protected override GameObject InstantiateCard()
    {
        Debug.Log("NetworkedGameController InstantiateCard");
        
        return Network.Instantiate(Prefab, Prefab.transform.position, Prefab.transform.rotation, 0) as GameObject;
    }

    protected override IEnumerator DeselectCards(CardController one, CardController two)
    {
        yield return StartCoroutine(base.DeselectCards(one, two));

        Debug.Log("NetworkedGameController DeselectCards: " + one.Id + " & " + two.Id);

        one.networkView.RPC("QueueFlip", RPCMode.Others);
        two.networkView.RPC("QueueFlip", RPCMode.Others);
    }

    protected override void HandleCardMatch(int answer)
    {
        Debug.Log("NetworkGameController HandleCardMatch");

        MatchedAnswers[answer] = Network.time;

        networkView.RPC("HandleOpponentCardMatch", RPCMode.Others, answer);
    }

    protected override void HandleGameOver()
    {
        Debug.Log("NetworkedGameController HandleGameOver");

        NotificationCenter.DefaultCenter.PostNotification(this, "GameOver");

        networkView.RPC("HandleOpponentGameOver", RPCMode.Others);
    }

    [RPC, UsedImplicitly]
    private void HandleOpponentGameOver(NetworkMessageInfo info)
    {
        Debug.Log("NetworkedGameController HandleOpponentGameOver");

        NotificationCenter.DefaultCenter.PostNotification(this, "GameOver");
    }

    [RPC, UsedImplicitly]
    private void HandleOpponentCardMatch(int answer, NetworkMessageInfo info)
    {
        var timeOfMatch = MatchedAnswers[answer];

        if (timeOfMatch < 1 || timeOfMatch > info.timestamp)
        {
            Debug.Log("NetworkGameController HandleOpponentCardMatch: Opponent was first");

            networkView.RPC("HandlePlayerScoredFirst", info.sender);
        }
        else
        {
            Debug.Log("NetworkGameController HandleOpponentCardMatch: Opponent was second");
        }
    }

    [RPC, UsedImplicitly]
    private void HandlePlayerScoredFirst()
    {
        Debug.Log("NetworkGameController HandlePlayerScoredFirst");
    }

    protected new void GenerateAnswerKey()
    {
        base.GenerateAnswerKey();

        networkView.RPC("SetAnswerKey", RPCMode.Others, AnswerKey);
    }

    [RPC]
    protected void SetAnswerKey(int[] answerKey)
    {
        Debug.Log("NetworkedGameController SetAnswerKey");

        AnswerKey = answerKey;
    }

    [UsedImplicitly]
    private void OnPlayerDisconnected(NetworkPlayer player)
    {
        Debug.Log("NetworkedGameController OnPlayerDisconnected");

        Network.Disconnect();
        MasterServer.UnregisterHost();
    }

    [UsedImplicitly]
    private void OnDisconnectedFromServer(NetworkDisconnection info)
    {
        Debug.Log("NetworkedGameController OnDisconnectedFromServer: " + info);

        Application.LoadLevel("MenuScene");
    }
}
