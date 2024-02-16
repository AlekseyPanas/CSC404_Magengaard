using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerDetector: NetworkBehaviour
{
    public event Action OnPlayerEnter;

    void OnTriggerEnter(Collider col){
        if (col.CompareTag("Player")){
            OnPlayerEnter?.Invoke();
        }
    }
}
