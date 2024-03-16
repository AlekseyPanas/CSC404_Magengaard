using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerDetector: NetworkBehaviour
{
    public event Action<GameObject> OnPlayerEnter;
    [SerializeField] private GameObject player;
    [SerializeField] bool playerHasEntered;

    void OnTriggerEnter(Collider col){
        if (col.CompareTag("Player")){
            player = col.gameObject;
            playerHasEntered = true;
            OnPlayerEnter?.Invoke(col.gameObject);
        }
    }

    void OnTriggerExit(Collider col){
        if (col.CompareTag("Player")){
            player = null;
            playerHasEntered = false;
        }
    }

    public GameObject GetPlayer(){
        if (playerHasEntered && player!=null) return player;
        return null;
    }
}
