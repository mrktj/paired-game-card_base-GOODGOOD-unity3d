using System.Text;
using JetBrains.Annotations;
using UnityEngine;
using System.Collections;

public class ScreenManager : MonoBehaviour
{
    public static readonly int Rows = 3;
    public static readonly int Cols = 4;

    private int _lastWidth, _lastHeight;
    private bool _stay = true;

    [UsedImplicitly]
	private void Awake() 
    {
        Debug.Log("ScreenManager Awake");
        
		NotificationCenter.DefaultCenter.AddObserver(this, "OnCardInitialized");

        DontDestroyOnLoad(transform.gameObject);

        StartCoroutine(CheckForResize());
    }

    IEnumerator CheckForResize()
    {
        _lastWidth = Screen.width;
        _lastHeight = Screen.height;

        while (_stay)
        {
            if (_lastWidth != Screen.width || _lastHeight != Screen.height)
            {
                _lastWidth = Screen.width;
                _lastHeight = Screen.height;

                RepositionCards();
            }
            yield return new WaitForSeconds(0.3f);
        }
    }

    [UsedImplicitly]
    void OnCardInitialized(NotificationCenter.Notification notification)
    {
        Debug.Log("ScreenManager OnCardInitialized: " + ((CardController)notification.sender).Id);

        PositionCard(notification.sender.gameObject);
    }

    void RepositionCards()
    {
        Debug.Log("ScreenManager RepositionCards");
        
        var controllers = FindObjectsOfType<CardController>();
        foreach (var c in controllers)
        {
            var card = c.gameObject;
            PositionCard(card);
        }
    }

    public void PositionCard(GameObject card)
    {
        var controller = card.GetComponent<CardController>();

        var gt = controller.Player ? "PlayerGrid" : "OpponentGrid";
        var grid = GameObject.FindGameObjectWithTag(gt);

        var x = controller.Id%Cols;
        var y = controller.Id/Cols;
        
        var target = grid.transform;
        var widget = grid.GetComponent<UIWidget>();

        var cellWidth = widget.width/Cols;
        var cellHeight = widget.height/Rows;

        var spriteWidth = cellWidth*0.9f;
        var spriteHeight = cellHeight*0.9f;

        var t = card.transform;
        t.parent = target;
        t.localPosition = Vector3.zero;
        t.localRotation = Quaternion.identity;
        t.localScale = Vector3.one;

        var sprite = card.GetComponent<UISprite>();

        sprite.bottomAnchor.target = target;
        sprite.topAnchor.target = target;
        sprite.leftAnchor.target = target;
        sprite.rightAnchor.target = target;

        sprite.bottomAnchor.relative = 0;
        sprite.topAnchor.relative = spriteHeight/widget.height;
        sprite.leftAnchor.relative = 0;
        sprite.rightAnchor.relative = spriteWidth/widget.width;

        sprite.UpdateAnchors();
        sprite.ResetAnchors();

        sprite.bottomAnchor.absolute = cellHeight*y;
        sprite.topAnchor.absolute = cellHeight*y;
        sprite.leftAnchor.absolute = cellWidth*x;
        sprite.rightAnchor.absolute = cellWidth*x;
    }

    [UsedImplicitly]
    void OnDestroy()
    {
        _stay = false;
    }
}
