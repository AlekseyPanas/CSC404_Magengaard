using Unity.Netcode;
using UnityEngine;


public class PickupSystem: NetworkBehaviour {

    private enum PickupState {
        NONE = 0,
        WALKING = 1,
        PICKUP = 2,
        INSPECTION = 3,
        POCKETING = 4,
        UNPOCKETING = 5
    }

    [SerializeField] private Movement _movementSystem;
    private PickupState _state = PickupState.NONE;

    public void Awake() {
        _movementSystem.arrivedAtTarget += () => {
            if (_state == PickupState.WALKING) { _state = PickupState.PICKUP; }
        };
    }

    public void OnTriggerEnter(Collider other) {
        if (other.gameObject.tag == "Pickupable" && _state == PickupState.NONE) {
            _movementSystem.setPupperModeMoveTarget(other.transform.gameObject.transform.gameObject.transform.position);
            _movementSystem.setPuppetMode(true);
        }
    }
}
