using System;
using Unity.Netcode;
using UnityEngine;

/** Attaches to the fbx model (i.e the same object as the animator) to listen for animation events and broadcast them
to the parent game object */
public class AnimationEventReceiver: NetworkBehaviour {

    public event Action OnFinishedPickupEvent = delegate { };
    public event Action OnItemPickedUpEvent = delegate { };

    public void OnPickupFinish() {
        Debug.Log("Receiver for animation");
        OnFinishedPickupEvent();
    }

    public void OnPickupItem() {
        Debug.Log("Receiver for animation");
        OnItemPickedUpEvent();
    }
}
