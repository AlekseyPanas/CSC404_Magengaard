using System;
using Unity.Netcode;
using UnityEngine;

/** Attaches to the fbx model (i.e the same object as the animator) to listen for animation events and broadcast them
to the parent game object */
public class AnimationEventReceiver: NetworkBehaviour {

    public event Action OnFinishedPickupEvent = delegate { };  // When pickup animation finishes
    public event Action OnItemPickedUpEvent = delegate { };  // When the pickup animation is at a point that hand is touching ground
    public event Action OnPocketingFinishedEvent = delegate { };  // When pocketing animation finishes
    public event Action OnUnpocketingFinishedEvent = delegate { };  // When unpocketing animation finishes
    public event Action OnFootstepEvent = delegate { };  // Whenever footstep is made during walking cycle

    public void OnPickupFinish() { OnFinishedPickupEvent(); }

    public void OnPickupItem() { OnItemPickedUpEvent(); }

    public void OnPocketFinish() { OnPocketingFinishedEvent(); }

    public void OnUnpocketFinish() { OnUnpocketingFinishedEvent(); }

    public void OnFootstep() { OnFootstepEvent(); }
}

// This class violates open-closed principle. Ideally each action-method pair is its own class. However, that would make too many files
