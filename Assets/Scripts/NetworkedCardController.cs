using JetBrains.Annotations;
using UnityEngine;

public class NetworkedCardController : CardController 
{
    [UsedImplicitly]
    private void OnSerializeNetworkView(BitStream stream, NetworkMessageInfo info)
    {
        var id = Id;

        stream.Serialize(ref id);

        if (stream.isReading)
        {
            Debug.Log("NetworkedCardController OnSerializeNetworkView: isReading");

            Id = id;
        }
    }

    protected override void Initialize()
    {
        base.Initialize();

        if (!networkView.isMine)
        {
            ((NetworkedGameController) GameController).NetworkedCardInitialized(Id);
        }
    }

    [UsedImplicitly]
    protected new void OnClick()
    {
        base.OnClick();

        if (!Player) return;
        if (Flipping || FaceUp) return;
        
        networkView.RPC("QueueFlip", RPCMode.Others);
    }

    [UsedImplicitly]
    private void OnNetworkInstantiate(NetworkMessageInfo info)
    {
        if (!networkView.isMine)
        {
            var opponentGrid = GameObject.FindGameObjectWithTag("OpponentGrid");
            opponentGrid.GetComponent<CardGridController>().AddCard(gameObject);
        }
    }
}
