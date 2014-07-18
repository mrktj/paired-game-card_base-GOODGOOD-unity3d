using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

public class CardGridController : MonoBehaviour
{
    private List<GameObject> _cardList;
    private UIPanel _panel;
    private float _lastWidth, _lastHeight;
    
    [UsedImplicitly]
    private void Awake ()
    {
        Debug.Log("CardGridController Awake");

        _cardList = new List<GameObject>();
        _panel = transform.parent.GetComponent<UIPanel>();

        _lastWidth = _panel.width;
        _lastHeight = _panel.height;

        StartCoroutine(ResizeCheck());
    }

    IEnumerator ResizeCheck()
    {
        while (true)
        {
            if (_lastWidth != _panel.width || _lastHeight != _panel.height)
            {
                _lastWidth = _panel.width;
                _lastHeight = _panel.height;
                
                Reposition();
            }

            yield return new WaitForSeconds(0.3f);
        }
    }

    public void AddCard(GameObject card)
    {
        if (card == null)
        {
            Debug.LogError("CardGridController AddCard: Error! Card gameobject is null");
            return;
        }

        Debug.Log("CardGridController AddCard");

        var t = card.transform;
        t.parent = transform;
        t.localPosition = Vector3.zero;
        t.localRotation = Quaternion.identity;
        t.localScale = Vector3.one;
        card.layer = transform.gameObject.layer;

        _cardList.Add(card);
    }

    [UsedImplicitly]
    public void Reposition()
    {
        Debug.Log("CardGridController RepositionCards");

        var grdw = _panel.width;
        var grdh = _panel.height;

        var wpad = grdw * 0.01;
        var hpad = grdh * 0.01;

        wpad = wpad * 2 * (AbstractGameController.Cols - 1);
        hpad = hpad * 2 * (AbstractGameController.Rows - 1);

        var cardw = (grdw - wpad) / AbstractGameController.Cols;
        var cardh = (grdh - hpad) / AbstractGameController.Rows;

        foreach (var c in _cardList)
        {
            c.GetComponent<UISprite>().width = (int) cardw;
            c.GetComponent<UISprite>().height = (int) cardh;
        }

        GetComponent<UIGrid>().cellWidth = (int) ((grdw / AbstractGameController.Cols) * 1.02);
        GetComponent<UIGrid>().cellHeight = (int) ((grdh / AbstractGameController.Rows) * 1.02);

        GetComponent<UIGrid>().Reposition();
    }
}
