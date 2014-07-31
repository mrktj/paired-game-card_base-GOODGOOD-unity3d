using System;
using System.Collections;
using System.Runtime.InteropServices;
using JetBrains.Annotations;
using UnityEngine;

public class NetworkedGameController : AbstractGameController
{
    private bool _initializationComplete;

    public override int[] AnswerKey
    {
        protected set
        {
            Debug.Log("NetworkedGameController AnswerKey set");

            if (Network.isServer)
            {
                networkView.RPC("SetAnswerKey", RPCMode.Others, value);
            }

            base.AnswerKey = value;
        }
        get { return base.AnswerKey; }
    }

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

    protected override void Initialize()
    {
        base.Initialize();

        Debug.Log("NetworkedPlayerGameController Initialize");
    }

    protected override void BeforeGameStart()
    {
        Debug.Log("NetworkedPlayerGameController BeforeGameStart");

        StartCoroutine(OpponentGrid.QueueReposition(() =>
        {
            PlayerGrid.Panel.alpha = 1;
            OpponentGrid.Panel.alpha = 1;

            // At this point Initialization is complete meaning that the AnswerKey, 
            // Player cards, and Opponent cards have all been created and synchronized
            _initializationComplete = true;

            if (Network.isServer)
            {
                networkView.RPC("GetSynchronizedStartTime", RPCMode.Others);
            }
        }));
    }

    [RPC, UsedImplicitly]
    private void GetSynchronizedStartTime()
    {
        Debug.Log("NetworkedGameController GetSynchronizedStartTime");

        StartCoroutine(GetSynchronizedStartTimeCoroutine());
    }

    private IEnumerator GetSynchronizedStartTimeCoroutine()
    {
        while (!_initializationComplete)
        {
            Debug.Log(_initializationComplete);
            
            yield return new WaitForSeconds(0.3f);
        }

        var startTime = (float) Network.time + Network.GetAveragePing(Network.player)*2;

        BeginGameAtTime(startTime);
        networkView.RPC("BeginGameAtTime", RPCMode.Others, startTime);
    }

    [RPC]
    private void BeginGameAtTime(float startTime)
    {
        Debug.Log("NetworkedGameController BeginGameAtTime");

        StartCoroutine(BeginGameAtTimeCoroutine(startTime));
    }

    private IEnumerator BeginGameAtTimeCoroutine(float startTime)
    {
        while (Network.time < startTime)
        {
            yield return new WaitForSeconds(0.0001f);
        }
        
        Debug.Log("NetworkedGameController BeginGameTime: Game Ready at " + Network.time + " (" + DateTime.Now + ")");
    }

    [UsedImplicitly]
    public void NetworkedCardInitialized(CardController card)
    {
        Debug.Log("NetworkedGameController NetworkedCardInitialized: " + card.Id);

        if (!card.networkView.isMine)
        {
            OpponentGrid.AddCard(card.gameObject);

            if (card.Id == Count - 1)
            {
                AfterInitialize();
            }
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
