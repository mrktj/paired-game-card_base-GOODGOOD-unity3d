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

        ((NetworkedGameController) GameController).NetworkedCardInitialized(this);
    }

    [UsedImplicitly]
    protected new void OnClick()
    {
        base.OnClick();

        if (!Player) return;
        if (Flipping || FaceUp) return;
        
        networkView.RPC("QueueFlip", RPCMode.Others);
    }
}
