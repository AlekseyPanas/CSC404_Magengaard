using Unity.Netcode;
using UnityEngine;

public delegate void AimingFinished(INetworkSerializable data);

/**
An aimsystem should be a prefab that contains complete behavior on startup meant to acquire
user input for aiming. 

For example, linear direction aim from the player should be a prefab which
generates an arrow visual from the player, takes mouse input to change arrow's direction, and listen
for the user to click. 

Once the user has "aimed" (i.e, provided enough data to populate data in type T),
this object should fire the AimingFinishedEvent and destroy itself.
*/
public abstract class AAimSystem: MonoBehaviour {
    public event AimingFinished AimingFinishedEvent;

    /** Called by SpellSystem to set the client-owned player's transform object */
    public abstract void setPlayerTransform(Transform playerTransform);

    protected void invokeAimingFinishedEvent(INetworkSerializable spellData) {AimingFinishedEvent(spellData);}
}
