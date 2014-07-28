using System;
using System.Collections;
using System.Collections.Generic;
using Assets.Scripts;
using JetBrains.Annotations;
using UnityEngine;

public class CardGridController : MonoBehaviour
{
    private List<GameObject> _cardList;
    private UIPanel _panel;
    private float _width, _height;
    
    [UsedImplicitly]
    private void Awake ()
    {
        Debug.Log("CardGridController Awake");

        _cardList = new List<GameObject>();
        _panel = transform.parent.GetComponent<UIPanel>();

        _width = _panel.width;
        _height = _panel.height;
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

    public IEnumerator QueueReposition(Action callback)
    {
        Debug.Log("CardGridController QueueReposition");

        var hasRepositioned = false;
//        var elapsed = Time.time;

        while (!hasRepositioned)
     
        {
            if (!Utils.NearlyEqual(_width, _panel.width, float.Epsilon) ||
                !Utils.NearlyEqual(_height, _panel.height, float.Epsilon))
            {
                Reposition();

                hasRepositioned = true;
                continue;
            }

//            elapsed += Time.deltaTime;

            yield return new WaitForSeconds(0.3f);
        }

        callback();

        yield return null;
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
        GetComponent<UIGrid>().cellHeight = (int) ((grdh / AbstractGameController.Rows) * 1);

        GetComponent<UIGrid>().Reposition();
    }
}
