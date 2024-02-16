using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;


public abstract class AToggleable: NetworkBehaviour {
    
    public bool IsOn {get; private set;}
    
    protected void setToggle(bool isOn) {
        IsOn = isOn;
    }
}
