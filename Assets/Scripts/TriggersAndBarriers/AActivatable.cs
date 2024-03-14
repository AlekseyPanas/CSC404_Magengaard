using System;
using UnityEngine;
using Unity.Netcode;

/*
* Objects such as elemental terminals that have three states: dormant, inactive, active, can inherit from this class. 
*/
public enum ActiveState {
    DORMANT = 0,
    INACTIVE = 1,
    ACTIVE = 2
}

public abstract class AActivatable : NetworkBehaviour {
    public ActiveState state = ActiveState.DORMANT;
    public event Action<AActivatable> OnActivate = delegate{};

    protected void SetStateDormant(){
        if (state != ActiveState.ACTIVE) {
            state = ActiveState.DORMANT;
        }
    }

    protected void SetStateInactive(){
        if (state != ActiveState.ACTIVE) {
            state = ActiveState.INACTIVE;
        }
    }
    
    protected void SetStateActive(){
        if (state != ActiveState.DORMANT) {
            OnActivate?.Invoke(this);
            state = ActiveState.ACTIVE;
        }
    }
}