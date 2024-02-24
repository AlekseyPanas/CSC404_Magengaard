using Unity.Netcode;
using UnityEngine;


public class PickupSystem: NetworkBehaviour {

    [SerializeField] private Movement movementSystem;

    public void OnTriggerEnter(Collider other) {
        if (other.gameObject.tag == "Pickupable") {
            Debug.Log(other.gameObject);
        }
    }
}
