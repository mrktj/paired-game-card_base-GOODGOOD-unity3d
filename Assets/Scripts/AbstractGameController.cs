using System.Collections;
using System.Linq;
using Assets.Scripts;
using JetBrains.Annotations;
using UnityEngine;

public abstract class AbstractGameController : MonoBehaviour
{
    public GameObject Prefab;
    public GameObject PlayerGrid, OpponentGrid;
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

        _count = ScreenManager.Rows*ScreenManager.Cols;

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
        var panel = grid.transform.parent;

        var grdw = panel.GetComponent<UIPanel>().width;
        var grdh = panel.GetComponent<UIPanel>().height;

        var wpad = grdw * 0.02;
        var hpad = grdh * 0.02;

        wpad = wpad * 2 * (ScreenManager.Cols - 1);
        hpad = hpad * 2 * (ScreenManager.Rows - 1);


        var cardw = (grdw - wpad) / ScreenManager.Cols;
        var cardh = (grdh - hpad) / ScreenManager.Rows;

        for (var i = 0; i < _count; i++)
        {
            var card = InstantiateCard();
            var controller = card.GetComponent<CardController>();

            card.GetComponent<UISprite>().width = (int) cardw;
            card.GetComponent<UISprite>().height = (int) cardh;

            controller.Player = forPlayer;
            controller.Id = i;

            Utils.AddChild(grid, card);
        }

        grid.GetComponent<UIGrid>().cellWidth = (int) ((grdw / ScreenManager.Cols) * 1.04);
        grid.GetComponent<UIGrid>().cellHeight = (int) ((grdh / ScreenManager.Rows) * 1.04);

        grid.GetComponent<UIGrid>().Reposition();
    }

    protected abstract GameObject InstantiateCard();
    protected abstract void HandleCardMatch(int answer);
    protected abstract void HandleGameOver();
}
