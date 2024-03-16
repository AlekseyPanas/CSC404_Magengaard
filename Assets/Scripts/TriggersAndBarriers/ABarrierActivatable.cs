using System;
using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;

public abstract class ABarrierActivatable : NetworkBehaviour {
    [SerializeField] List<AActivatable> activatables;

    public void Start(){
        foreach(AActivatable a in activatables){
            a.OnActivate += OnActivation;
        }
    }

    void OnActivation(AActivatable a){
        activatables.Remove(a);
        if(activatables.Count == 0){
            BarrierDisable();
        }
    }

    protected abstract void BarrierEnable();
    protected abstract void BarrierDisable();
}