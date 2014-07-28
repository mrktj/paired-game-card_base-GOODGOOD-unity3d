using System.Collections;
using JetBrains.Annotations;
using UnityEngine;

public class CardController : MonoBehaviour
{
    private UIAtlas _defaultCardAtlas;

    public bool Player;
    public AbstractGameController GameController;

    public bool FaceUp { get; private set; }
    public int Answer { get; private set; }

    protected bool Flipping;
    private int _flipQueue;

    private int _id = -1;
    public int Id
    { 
        set
        {
            if (_id < 0)
            {
                _id = value; 

                Initialize();
            }
        }
        get { return _id; }
    }

    [UsedImplicitly]
    private void Awake()
    {
        Debug.Log("CardController Awake");

        _defaultCardAtlas = GetComponent<UISprite>().atlas;
    }

    [UsedImplicitly]
    protected virtual void Initialize()
    {
        Debug.Log("CardController (" + Id + ") Initialize");
        
        GameController = FindObjectOfType<AbstractGameController>();

        Answer = GameController.AnswerKey[Id];
    }

    [UsedImplicitly]
    private void Update()
    {
        if (_flipQueue > 0 && !Flipping)
        {
            StartCoroutine(Flip());
        }
    }

    [UsedImplicitly]
    protected void OnClick()
    {
        Debug.Log("CardController (" + Id + ") OnClick");

        if (!Player) return;
        if (Flipping || FaceUp) return;

        GameController.SelectCard(this);

        QueueFlip();
    }

    [RPC]
    public void QueueFlip()
    {
        Debug.Log("CardController (" + Id + ") QueueFlip");

        _flipQueue++;
    }

    private IEnumerator Flip()
    {
        Debug.Log("CardController (" + Id + ") Flip");

        Flipping = true;

        var config = new GoTweenConfig().eulerAngles(new Vector3(0, 180), true);
        config.easeType = GoEaseType.CubicOut;
        var tween = new GoTween(transform, 0.5f, config);

        Go.addTween(tween);

        var y = transform.rotation.eulerAngles.y;
        var changed = false;

        while (tween.totalElapsedTime < tween.totalDuration)
        {
            if (!changed && transform.rotation.eulerAngles.y - y > 90)
            {
                Debug.Log("CardController (" + Id + ") Flip: Change sprite to " + (FaceUp ? "Back" : "Face"));

                var sprite = GetComponent<UISprite>();

                if (FaceUp)
                {
                    sprite.atlas = _defaultCardAtlas;
                    sprite.spriteName = sprite.atlas.GetListOfSprites()[0];
                }
                else
                {
                    sprite.atlas = GameController.CardSetAtlas;
                    sprite.spriteName = sprite.atlas.GetListOfSprites()[Answer];
                    sprite.flip = UISprite.Flip.Horizontally;
                }

                changed = true;
            }

            yield return tween.waitForCompletion();
        }

        FaceUp = !FaceUp;
        Flipping = false;
        _flipQueue--;

        Debug.Log("CardController (" + Id + ") Done Flipping: Face " + (FaceUp ? "Up" : "Down"));
    }
}
