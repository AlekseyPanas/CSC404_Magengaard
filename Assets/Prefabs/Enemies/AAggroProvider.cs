using System;
using UnityEngine;
using Unity.Netcode;

public class AAggroProvider: NetworkBehaviour
{
    public event Action<GameObject> AggroEvent;

    public void TriggerAggroEvent(GameObject g){
        if (!IsServer) return;
        AggroEvent?.Invoke(g);
    }
}
