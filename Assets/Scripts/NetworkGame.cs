using System;
using System.Collections;
using JetBrains.Annotations;
using UnityEngine;

public class NetworkGame : AbstractGame
{
    public CardGridController OpponentGrid;

    #region Initialization

    private bool _initializationComplete;

    public override int[] AnswerKey
    {
        protected set
        {
            if (Network.isServer)
            {
                networkView.RPC("SetAnswerKey", RPCMode.Others, value);
            }
            base.AnswerKey = value;
        }
    }

    [RPC, UsedImplicitly]
    protected void SetAnswerKey(int[] answerKey)
    {
        AnswerKey = answerKey;
    }

    [UsedImplicitly]
    protected override void Start()
    {
        base.Start();

        if (Network.isServer)
        {
            GenerateAnswerKey();
        }
    }

    [UsedImplicitly]
    public void OnNetworkedCardInitialized(AbstractCard card)
    {
        if (!card.networkView.isMine && card.Id == Count - 1)
        {
            AfterInitialize();
        }
    }
    
    protected override void BeforeGameReady()
    {
        StartCoroutine(OpponentGrid.QueueReposition(() =>
        {
            _initializationComplete = true;

            if (Network.isServer)
            {
                networkView.RPC("GetStartTime", RPCMode.Others);
            }
        }));
    }

    [RPC, UsedImplicitly]
    private void GetStartTime()
    {
        StartCoroutine(GetStartTimeCoroutine());
    }

    private IEnumerator GetStartTimeCoroutine()
    {
        while (!_initializationComplete)
        {
            yield return new WaitForSeconds(0.3f);
        }

        var startTime = (float) Network.time + Network.GetAveragePing(Network.player)*2;

        GameReady(startTime);
        networkView.RPC("GameReady", RPCMode.Others, startTime);
    }

    [RPC]
    private void GameReady(float startTime)
    {
        StartCoroutine(GameReadyCoroutine(startTime));
    }

    private IEnumerator GameReadyCoroutine(float startTime)
    {
        while (Network.time < startTime)
        {
            yield return new WaitForSeconds(0.0001f);
        }
        
        Debug.Log("NetworkedGameController BeginGameTime: Game Ready at " + Network.time + " (" + DateTime.Now + ")");
    }


    protected override GameObject InstantiateCard()
    {
        return Network.Instantiate(Prefab, Prefab.transform.position, Prefab.transform.rotation, 0) as GameObject;
    }


    #endregion

    #region Game Logic

    protected new IEnumerator DeselectCards(AbstractCard one, AbstractCard two)
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

    #endregion

    #region Network Logic

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

    #endregion
}
