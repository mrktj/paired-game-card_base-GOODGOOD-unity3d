using System.Collections;
using System.ComponentModel;
using JetBrains.Annotations;
using UnityEngine;

public abstract class AbstractCard : MonoBehaviour
{
    public UIAtlas FaceAtlas, BackAtlas;

    public int Answer;
    public bool Player;

    public bool FaceUp;

    private bool _flipping;
    private int _flipQueue;

    #region Initialization

    private int _id = -1;

    public int Id {
        set
        {
            if (_id == -1)
            {
                Id = value;
                Initialize();
            }
        }
        get { return _id; }
    }

    protected abstract void Initialize();

    #endregion

    #region Game Logic

    [UsedImplicitly]
    private void Update()
    {
        if (_flipQueue > 0 && !_flipping)
        {
            StartCoroutine(Flip());
        }
    }

    [RPC]
    public void QueueFlip()
    {
        _flipQueue++;
    }

    private IEnumerator Flip()
    {
        _flipping = true;

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
                var sprite = GetComponent<UISprite>();

                if (FaceUp)
                {
                    sprite.atlas = BackAtlas;
                    sprite.spriteName = sprite.atlas.GetListOfSprites()[0];
                }
                else
                {
                    sprite.atlas = FaceAtlas;
                    sprite.spriteName = sprite.atlas.GetListOfSprites()[Answer];
                    sprite.flip = UISprite.Flip.Horizontally;
                }

                changed = true;
            }

            yield return tween.waitForCompletion();
        }

        FaceUp = !FaceUp;
        _flipping = false;
        _flipQueue--;
    }

    #endregion
}
