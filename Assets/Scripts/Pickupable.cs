using System;
using Unity.Netcode;
using UnityEngine;

/** Attach to an object that is meant to be picked up by the player. This class enforces the correct format
on the pickupable object (ensures the object has the right tag and components), as well as provides configuration. */
public class Pickupable: NetworkBehaviour {
    public float stopRadius;  // How far away from the object's position the player should stop before picking it up
    public bool doInspect;  // If true, indicates to the pickup system that the inspection loop should play indefinitely until some script 
    // disables it. This is intended for UI systems which listen for the pickup system and react
    [SerializeField] private GameObject inspectable = null;  // A gameobject that contains an IInspectable component. Used to link Pickupables to UI elements. Can be null for instant pocketing
    public IInspectable Inspectable {get {
        if (inspectable == null) { return null; }

        var insp = inspectable.GetComponent<IInspectable>();
        if (insp == null) { throw new Exception("Pickupable.inspectable must inject a game object that implements IInspectable"); }
        return insp;
    }}

    // Ensures correct configuration
    void Start() {
        if (gameObject.tag != "Pickupable") { throw new Exception("A Pickupable must be tagged with 'Pickupable'"); }
        if (GetComponent<Collider>() == null) { throw new Exception("A Pickupable must have a collider zone to initiate pickup sequence"); }
    }

    /** Allows anyone to change the transform of this pickupable. This is used by the pickup system to attach object to player's hand */
    [ServerRpc(RequireOwnership = false)]
    public void ChangeTransformServerRpc(Vector3 position, Quaternion orientation) {
        transform.position = position;
        transform.rotation = orientation;
    }
}
