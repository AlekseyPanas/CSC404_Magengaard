using Unity.Netcode;
using UnityEngine;

public class Direction2DSpellParams: INetworkSerializable {

    private Vector2 direction2d;
    public Vector2 Direction2D {get { return direction2d; } }

    public Direction2DSpellParams(Vector2 direction2d) { this.direction2d = direction2d; }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter { serializer.SerializeValue(ref direction2d); }
}
