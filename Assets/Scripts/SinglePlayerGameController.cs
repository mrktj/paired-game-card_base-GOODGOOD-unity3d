using System;
using JetBrains.Annotations;
using UnityEngine;

public class SinglePlayerGameController : AbstractGame
{
    protected new void Start()
    {
        base.Start();

        Debug.Log("SinglePlayerGameController Start");

        GenerateAnswerKey();
    }

    protected override void Initialize()
    {
        base.Initialize();

        Debug.Log("SinglePlayerGameController Initialize");

        GenerateOpponentCards();
    }

    protected override void BeforeGameStart()
    {
        Debug.Log("SinglePlayerGameController BeforeGameStart");

        StartCoroutine(OpponentGrid.QueueReposition(() =>
        {
            PlayerGrid.Panel.alpha = 1;
            OpponentGrid.Panel.alpha = 1;
        }));
    }


    protected override void GenerateOpponentCards()
    {
        base.GenerateOpponentCards();

        AfterInitialize();
    }

    protected override void HandleCardMatch(int answer)
    {
        Debug.Log("SinglePlayerGameController HandleCardMatch");

        MatchedAnswers[answer] = DateTime.Now.Millisecond;
    }

    protected override void HandleGameOver()
    {
        Debug.Log("SinglePlayerGameController HandleGameOver");

        NotificationCenter.DefaultCenter.PostNotification(this, "GameOver");
    }

    protected override GameObject InstantiateCard()
    {
        Debug.Log("SinglePlayerGameController InstantiateCard");

        return Instantiate(Prefab) as GameObject;
    }
}
