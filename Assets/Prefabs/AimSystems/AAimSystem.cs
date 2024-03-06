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
this object should fire the AimingFinishedEvent.

An aim system should not destroy itself, and is destroyed by the spell system class.
*/
public abstract class AAimSystem: MonoBehaviour {
    public LayerMask layerMask;
    public event AimingFinished AimingFinishedEvent = delegate {};

    /** Called by SpellSystem to set the client-owned player's transform object */
    public abstract void setPlayerTransform(Transform playerTransform);

    /** Call this method internally in inheritting classes once aiming has finished */
    protected void invokeAimingFinishedEvent(SpellParamsContainer spellData) {AimingFinishedEvent(spellData);}

    protected void OnDestroy() {
        //Debug.Log("\t\t\t\t\t\t\tOnDestroy from AAimsystem");
        // Unsubscribe all subscribers from event
        if (AimingFinishedEvent != null) {
            foreach (var d in AimingFinishedEvent.GetInvocationList()) {
                AimingFinishedEvent -= d as AimingFinished;
            }
        }
        PlayerDeathController.OnDeath -= DestroySelf;
    }
    protected void DestroySelf(){
        Destroy(gameObject);
    }

    protected void Start(){
        PlayerDeathController.OnDeath += DestroySelf;
    }
}
