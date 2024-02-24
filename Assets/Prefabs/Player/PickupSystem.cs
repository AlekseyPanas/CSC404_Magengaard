using Unity.Netcode;
using UnityEngine;


public class PickupSystem: NetworkBehaviour {

    private static readonly int ANIM_PICKUP = Animator.StringToHash("Pickup");

    private enum PickupState {
        NONE = 0,
        WALKING = 1,
        PICKUP = 2,
        PICKEDUP = 3,
        INSPECTION = 4,
        POCKETING = 5,
        UNPOCKETING = 6
    }

    [Tooltip("Animator of the child fbx model")]
    [SerializeField] private Animator _animator;

    [Tooltip("Event receiver script attached to child fbx mode (same as animator)")]
    [SerializeField] private AnimationEventReceiver _eventReceiver;
    
    [Tooltip("object within the rig which represents the hand that will hold the picked up item")]
    [SerializeField] private Transform _handLocation;
    
    private Movement _movementSystem;  // Used to set puppet mode
    private PickupState _state = PickupState.NONE;  // State machine for transitioning through pickup sequence
    private Pickupable _objectBeingPickedUp = null;  // Tracks game object which is being held

    void Awake() {
        _movementSystem = GetComponent<Movement>();
        _movementSystem.arrivedAtTarget += () => {
            if (_state == PickupState.WALKING) { 
                _state = PickupState.PICKUP; 
                _animator.SetBool(ANIM_PICKUP, true);
                Debug.Log("Finished walking"); 
            }
        };

        _eventReceiver.OnFinishedPickupEvent += () => {
            Debug.Log("Pickup Finished");
            _animator.SetBool(ANIM_PICKUP, false);
        };

        _eventReceiver.OnItemPickedUpEvent += () => {
            _state = PickupState.PICKEDUP;
        };
    }

    void OnTriggerEnter(Collider other) {
        if (!IsOwner) { return; }

        if (other.gameObject.tag == "Pickupable" && _state == PickupState.NONE) {
            // Sets target position to be 'stopRadius' away from the pickupable (in direction of player)
            _movementSystem.setPuppetModeMoveTarget(other.transform.position + 
                                                (transform.position - other.transform.position).normalized * 
                                                other.GetComponent<Pickupable>().stopRadius);
            _movementSystem.setPuppetMode(true);
            _objectBeingPickedUp = other.gameObject.GetComponent<Pickupable>();
            _state = PickupState.WALKING;
        }
    }

    void Update() {
        if (_state >= PickupState.PICKEDUP) {
            _objectBeingPickedUp.ChangeTransformServerRpc(_handLocation.transform.position, _handLocation.transform.rotation);
        }
    }
}
