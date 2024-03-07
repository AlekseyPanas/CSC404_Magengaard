
using System;
using UnityEngine;

/** Inheritors of this class (Usually client-side UI elements) react to when the corresponding pickupable is being inspected
by the player. PickupSystem calls methods in this interface while a Pickupable object is injected with a gameobject containing
an IInspectable component */
public interface IInspectable {

    /** Called by the PickupSystem when the Pickupable injected with this IInspectable was picked up and is being inspected.
    The PickupSystem will provide a callback (OnInspectEnd) which must be called once inspection is finished (releasing the player) */
    void OnInspectStart(ControllerRegistrant registrant);
}