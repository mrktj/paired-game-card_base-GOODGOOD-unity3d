using System;
using System.Collections;
using System.Linq;
using Assets.Scripts;
using JetBrains.Annotations;
using UnityEngine;

public abstract class AbstractGameController : MonoBehaviour
{
    public static int Cols = 4;
    public static int Rows = 3;

    public GameObject Prefab;
    public CardGridController PlayerGrid;
    public CardGridController OpponentGrid;
    public UIAtlas CardSetAtlas;

    protected int Count;
    protected double[] MatchedAnswers;

    private CardController _selectedCardOne;
    private CardController _selectedCardTwo;


    /// <summary>
    /// Store for the AnswerKey property
    /// </summary>
    private int[] _answerKey;

    /// <summary>
    /// AnswerKey property 
    /// </summary>
    /// <value>
    /// Holds the game's answer key. It is either generated or received
    /// from a peer that is acting as a server. The game is considered 
    /// ready to initialze at this point to ensure that the client and
    /// server are in sync.
    /// </value>
    public virtual int[] AnswerKey
    {
        protected set
        {
           Debug.Log("AbstractGameController AnswerKey set");

            if (_answerKey == null)
            {
                _answerKey = value;

                Initialize();
            }
        }
        get { return _answerKey; }
    }

    [UsedImplicitly]
    protected void Start() 
    {
        Debug.Log("AbstractGameController Start");

        Count = Cols * Rows;

        MatchedAnswers = new double[Count/2];
    }

    /// <summary>
    /// Initialize is called when the answer key has been generated or 
    /// received from the peer. It is responsible for generating the cards.
    /// </summary>
    protected virtual void Initialize()
    {
        Debug.Log("AbstractGameController Initialize");

        GeneratePlayerCards();
    }

    protected void GameReady()
    {
        StartCoroutine(PlayerGrid.QueueReposition(() => StartCoroutine(OpponentGrid.QueueReposition(() =>
        {
            PlayerGrid.transform.parent.GetComponent<UIPanel>().alpha = 1;
            OpponentGrid.transform.parent.GetComponent<UIPanel>().alpha = 1;

            Debug.Log("AbstractGameController GameReady: " + DateTime.Now);
        }))));
    }

    private void CheckSelectedCards()
    {
        if (_selectedCardOne == null || _selectedCardTwo == null) return;

        var answer = AnswerKey[_selectedCardOne.Id];
        

        if (answer == AnswerKey[_selectedCardTwo.Id])
        {
            Debug.Log("AbstractGameController CheckSelectedCards: True");

            HandleCardMatch(answer);

            if (MatchedAnswers.All(value => value > 0))
            {
                HandleGameOver();
            }
        }
        else
        {
            Debug.Log("AbstractGameController CheckSelectedCards: False");

            StartCoroutine(DeselectCards(_selectedCardOne, _selectedCardTwo));
        }

        _selectedCardOne = null;
        _selectedCardTwo = null;
    }

    public void SelectCard(CardController card)
    {
        Debug.Log("AbstractGameController SelectCard: " + card.Id);

        if (_selectedCardOne != null && _selectedCardTwo != null) return;

        if (_selectedCardOne == null)
        {
            _selectedCardOne = card;
        }
        else
        {
            if (_selectedCardOne == card) return;

            _selectedCardTwo = card;

            CheckSelectedCards(); 
        }
    }

    protected virtual IEnumerator DeselectCards(CardController one, CardController two)
    {
        Debug.Log("AbstractGameController DeselectCards: " + one.Id + " & " + two.Id);

        while (!two.FaceUp) yield return null;

        one.QueueFlip();
        two.QueueFlip();
    }


    protected void GenerateAnswerKey()
    {
        Debug.Log("AbstractGameController GenerateAnswerKey");

        int[] answerKey = new int[Count];

        for (int i = 0, j = Count/2; i < Count/2; i++, j++)
        {
            answerKey[i] = i;
            answerKey[j] = i;
        }

        Utils.Shuffle(answerKey);

        // Calls overridden setter
        AnswerKey = answerKey;
    }

    private void GeneratePlayerCards()
    {
        Debug.Log("AbstractGameController GeneratePlayerCards");

        GenerateCards(true);
    }

    protected virtual void GenerateOpponentCards()
    {
        Debug.Log("AbstractGameController GenerateOpponentCards");

        GenerateCards(false);
    }

    private void GenerateCards(bool forPlayer)
    {
        var grid = forPlayer ? PlayerGrid : OpponentGrid;

        for (var i = 0; i < Count; i++)
        {
            var card = InstantiateCard();
            var controller = card.GetComponent<CardController>();

            controller.Player = forPlayer;
            controller.Id = i;

            grid.AddCard(card);
        }
    }

    protected abstract GameObject InstantiateCard();
    protected abstract void HandleCardMatch(int answer);
    protected abstract void HandleGameOver();
}
