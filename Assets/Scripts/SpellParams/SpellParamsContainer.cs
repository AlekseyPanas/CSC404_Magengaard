using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

/** A bloated datastructure which is passed over the network as a spell parameter. This structure should contains space for data necessary for every existing
spell parameters class */
public class SpellParamsContainer: INetworkSerializable {

    private static readonly int SIZE = 5;

    private Vector3[] vectors3D = new Vector3[SIZE];
    private float[] floats = new float[SIZE];

    /** Use this to set the floats necessary for the corresponding spell params class */
    public SpellParamsContainer setFloat(int index, float val) { floats[index] = val; return this; }
    /** Use this to set the vectors necessary for the corresponding spell params class */
    public SpellParamsContainer setVector3(int index, Vector3 val) { vectors3D[index] = val; return this; }

    public float getFloat(int index) { return floats[index]; }
    public Vector3 getVector3(int index) { return vectors3D[index]; }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter { 
        for (int i = 0; i < SIZE; i++) {
            serializer.SerializeValue(ref vectors3D[i]);  
            serializer.SerializeValue(ref floats[i]);
        }
    }
}