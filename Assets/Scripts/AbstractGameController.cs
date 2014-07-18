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

    private int _count;
    protected double[] MatchedAnswers;

    private CardController _selectedCardOne;
    private CardController _selectedCardTwo;


    private int[] _answerKey;
    public int[] AnswerKey
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

        _count = Cols * Rows;

        MatchedAnswers = new double[_count/2];
    }

    protected virtual void Initialize()
    {
        Debug.Log("AbstractGameController Initialize");

        GeneratePlayerCards();
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

        int[] answerKey = new int[_count];

        for (int i = 0, j = _count/2; i < _count/2; i++, j++)
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

    protected void GenerateOpponentCards()
    {
        Debug.Log("AbstractGameController GenerateOpponentCards");

        GenerateCards(false);
    }

    private void GenerateCards(bool forPlayer)
    {
        var grid = forPlayer ? PlayerGrid : OpponentGrid;

        for (var i = 0; i < _count; i++)
        {
            var card = InstantiateCard();
            var controller = card.GetComponent<CardController>();

            controller.Player = forPlayer;
            controller.Id = i;

            grid.AddCard(card);
        }

        grid.Reposition();
    }

    protected abstract GameObject InstantiateCard();
    protected abstract void HandleCardMatch(int answer);
    protected abstract void HandleGameOver();
}
