using Unity.Netcode;
using UnityEngine;

/** 
* Used by spells which take no parameters
*/
public class BlankSpellParams: INetworkSerializable {
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter { }
}
