using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public abstract class BarrierToggle: NetworkBehaviour {
    protected abstract void OnBarrierEnable();
    protected abstract void OnBarrierDisable();
}
