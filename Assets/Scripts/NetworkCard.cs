using JetBrains.Annotations;
using UnityEngine;

public class NetworkCard : AbstractCard
{
    [UsedImplicitly]
    private void OnSerializeNetworkView(BitStream stream, NetworkMessageInfo info)
    {
        var id = Id;

        stream.Serialize(ref id);

        if (stream.isReading)
        {
            Id = id;
        }
    }

    protected override void Initialize()
    {

    }
}
