using System;
using Unity.Netcode;
using UnityEngine;

/** Attach to an object that is meant to be picked up by the player. This class enforces the correct format
on the pickupable object (ensures the object has the right tag and components), as well as provides configuration. */
public class Pickupable: NetworkBehaviour {
    public float stopRadius;  // How far away from the object's position the player should stop before picking it up

    // Ensures correct configuration
    void Start() {
        if (gameObject.tag != "Pickupable") { throw new Exception("A Pickupable must be tagged with 'Pickupable'"); }
        if (GetComponent<Collider>() == null) { throw new Exception("A Pickupable must have a collider zone to initiate pickup sequence"); }
    }

    /** Allows anyone to change the transform of this pickupable. This is used by the pickup system to attach object to player's hand */
    [ServerRpc(RequireOwnership = false)]
    public void ChangeTransformServerRpc(Vector3 position, Quaternion orientation) {
        Debug.Log("Transform being set");
        transform.position = position;
        transform.rotation = orientation;
    }
}
