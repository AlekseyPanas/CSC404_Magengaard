using Unity.Netcode;
using UnityEngine;

public delegate void AimingFinished(SpellParamsContainer data);

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

    /** Call this method internally in inheritting classes once aiming has finished */
    protected void invokeAimingFinishedEvent(SpellParamsContainer spellData) {AimingFinishedEvent(spellData);}

    /** Called by spell system to disable or enable input. For example, when the user has dragged the mouse long enough,
    gesture drawing has begun and this method will be called to disable the aim system. You may also consider disabling visuals
    in this method when that happens */
    public abstract void toggleInput(bool doAcceptInput);
}
