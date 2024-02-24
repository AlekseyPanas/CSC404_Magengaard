using System;
using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;

public abstract class ABarrierActivatable : NetworkBehaviour {
    [SerializeField] List<GameObject> activatables;

    void Start(){
        foreach(GameObject g in activatables){
            if(g.TryGetComponent<AActivatable>(out var i)){
                i.OnActivate += OnActivation;
            } else {
                Debug.Log(name + " ABarrierActivatable: attempted to inject an object that doesn't ineherit from AActivatable");
            }
        }
    }

    void OnActivation(GameObject g){
        activatables.Remove(g);
        if(activatables.Count == 0){
            BarrierEnable();
        }
    }

    protected abstract void BarrierEnable();
    protected abstract void BarrierDisable();
}