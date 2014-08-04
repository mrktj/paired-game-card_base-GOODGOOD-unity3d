using System;
using System.Collections;
using System.Linq;
using Assets.Scripts;
using JetBrains.Annotations;
using UnityEngine;

public abstract class AbstractGame : MonoBehaviour
{
    public static int Cols = 4;
    public static int Rows = 3;

    protected int Count = Cols*Rows;

    public GameObject Prefab;
    public UIAtlas CardSetAtlas;

    public CardGridController PlayerGrid;

    #region Initialization

    /// <summary>
    /// Store for the AnswerKey property
    /// </summary>
    private int[] _answerKey;

    /// <summary>
    /// Used to check for matches based on the selected cards.
    /// This is either generated client side or recieved by a peer.
    /// </summary>
    /// <value>
    /// Each index in the array represents a card's value
    /// based on it's ID.
    /// </value>
    public virtual int[] AnswerKey
    {
        protected set
        {
            if (_answerKey == null)
            {
                _answerKey = value;

                Initialize();
            }
        }
        get { return _answerKey; }
    }

    /// <summary>
    /// Generates an answer key for the game.
    /// </summary>
    protected void GenerateAnswerKey()
    {
        int[] answerKey = new int[Count];

        for (int i = 0, j = Count/2; i < Count/2; i++, j++)
        {
            answerKey[i] = i;
            answerKey[j] = i;
        }
        Utils.Shuffle(answerKey);

        AnswerKey = answerKey; // Calls overridden setter
    }

    /// <summary>
    /// This is the first call in the initialization chain. 
    /// </summary>
    [UsedImplicitly]
    protected virtual void Start() 
    {
        MatchedAnswers = new double[Count/2];
    }

    /// <summary>
    /// Called after the answer key has been
    /// generated or received from the peer. 
    /// </summary>
    protected virtual void Initialize()
    {
        GenerateCards(true);
    }

    /// <summary>
    /// Called after all initialization has been completed.
    /// </summary>
    protected void AfterInitialize()
    {
        StartCoroutine(PlayerGrid.QueueReposition(BeforeGameReady));
    }

    /// <summary>
    /// The last step in the initialization chain.
    /// </summary>
    protected abstract void BeforeGameReady();

    /// <summary>
    /// Custom instantiation for card gameObjects.
    /// </summary>
    protected abstract GameObject InstantiateCard();

    /// <summary>
    /// A helper function that provides a way for 
    /// subclasses to generate opponent cards.
    /// </summary>
    protected virtual void GenerateOpponentCards()
    {
        GenerateCards(false);
    }

    /// <summary>
    /// Generates a set of cards for the game.
    /// </summary>
    private void GenerateCards(bool forPlayer)
    {
        for (var i = 0; i < Count; i++)
        {
            var card = InstantiateCard();
            var controller = card.GetComponent<AbstractCard>();

            controller.Player = forPlayer;
            controller.Id = i;
        }
    }

    #endregion

    #region Game Logic

    protected double[] MatchedAnswers;

    private AbstractCard _selectedCardOne;
    private AbstractCard _selectedCardTwo;

    public void SelectCard(AbstractCard card)
    {
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

    private void CheckSelectedCards()
    {
        if (_selectedCardOne == null || _selectedCardTwo == null) return;

        var answer = AnswerKey[_selectedCardOne.Id];
        

        if (answer == AnswerKey[_selectedCardTwo.Id])
        {
            HandleCardMatch(answer);

            if (MatchedAnswers.All(value => value > 0))
            {
                HandleGameOver();
            }
        }
        else
        {
            StartCoroutine(DeselectCards(_selectedCardOne, _selectedCardTwo));
        }

        _selectedCardOne = null;
        _selectedCardTwo = null;
    }
    
    protected abstract void HandleCardMatch(int answer);
    protected abstract void HandleGameOver();

    protected virtual IEnumerator DeselectCards(AbstractCard one, AbstractCard two)
    {
        while (!two.FaceUp) yield return null;

        one.QueueFlip();
        two.QueueFlip();
    }

    #endregion
}
