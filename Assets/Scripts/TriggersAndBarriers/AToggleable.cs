using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;


/** Extend this class for any toggleable game switch or object. Call setToggle in the inheriting class to change the state and fire the event */
public abstract class AToggleable: NetworkBehaviour {
    
    public event Action<bool> changedToggleEvent = delegate {};

    public bool IsOn {get; private set;}
    
    public void setToggle(bool isOn) {
        if (IsOn != isOn) { changedToggleEvent(isOn); }
        IsOn = isOn;
    }
}
