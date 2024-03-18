using System;
using FMODUnity;
using Unity.Netcode;
using UnityEngine;

public enum PickupablesNetworkPrefabListIndexes {
    BOOK = 0,
    SPELLPAGE = 1
}

/** Attach to an object that is meant to be picked up by the player. This class enforces the correct format
on the pickupable object (ensures the object has the right tag and components), as well as provides configuration. */
public class Pickupable: NetworkBehaviour {
    
    public float stopRadius;  // How far away from the object's position the player should stop before picking it up
    [SerializeField] private GameObject inspectable = null;  // A gameobject that contains an IInspectable component. Used to link Pickupables to UI elements. Can be null for instant pocketing
    [SerializeField] private Quaternion orientationOffset = Quaternion.identity;  // When held the hand of the player, this offset is applied to the rotation
    [SerializeField] private Vector3 positionOffset = Vector3.zero;  // When held the hand of the player, this offset is applied to the position

    public IInspectable Inspectable {get {
        if (inspectable == null) { return null; }

        var insp = inspectable.GetComponent<IInspectable>();
        if (insp == null) { throw new Exception("Pickupable.inspectable must inject a game object that implements IInspectable"); }
        return insp;
    }}

    /** Should be called once to set the inspectable if instantiating */
    public void SetInspectableGameObject(GameObject g) { inspectable = g; }

    /** Play the sound on pickup */
    public void PlayPickupSound() { 
        StudioEventEmitter emitter = GetComponent<StudioEventEmitter>();
        if (emitter != null) {
            emitter.Play();
            emitter.EventInstance.setVolume(0.5f);
        } 
    }

    // Ensures correct configuration
    void Start() {
        if (gameObject.tag != "Pickupable") { throw new Exception("A Pickupable must be tagged with 'Pickupable'"); }
        if (GetComponent<Collider>() == null) { throw new Exception("A Pickupable must have a collider zone to initiate pickup sequence"); }
    }

    /** Allows anyone to change the transform of this pickupable. This is used by the pickup system to attach object to player's hand */
    [ServerRpc(RequireOwnership = false)]
    public void ChangeTransformServerRpc(Vector3 position, Quaternion orientation) {
        transform.position = position + positionOffset;
        transform.rotation = orientation * orientationOffset;
    }

    /** Destroy over network*/
    [ServerRpc(RequireOwnership = false)]
    public void NetworkDestroySelfServerRpc() {
        Destroy(gameObject);
    }
}
