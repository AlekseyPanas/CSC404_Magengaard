using Unity.Netcode;
using UnityEngine;


/** Manages the sequence of picking up an approached pickupable. Also exposes methods to "unpocket" items for animating the character
holding something if they "take out" the item they previously picked up */
public class PickupSystem: NetworkBehaviour {

    private static readonly int ANIM_STATE = Animator.StringToHash("AnimState");

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
                _animator.SetInteger(ANIM_STATE, (int)AnimationStates.PICKUP);
                
                
                Debug.Log("Finished walking"); 
            }
        };

        // Once the full pickup sequence is finished
        _eventReceiver.OnFinishedPickupEvent += () => {
            Debug.Log("Pickup Finished");
            _animator.SetInteger(ANIM_STATE, (int)AnimationStates.INSPECT);
            var insp = _objectBeingPickedUp.Inspectable;
            if (insp == null) { _animator.SetInteger(ANIM_STATE, (int)AnimationStates.POCKET); }
            else {
                insp.OnInspectStart(() => {
                    _animator.SetInteger(ANIM_STATE, (int)AnimationStates.POCKET);
                });
                _animator.SetInteger(ANIM_STATE, (int)AnimationStates.INSPECT);
            }
        };

        // Event when the pickup animation is at a stage where the hand is touching the ground
        _eventReceiver.OnItemPickedUpEvent += () => {
            _state = PickupState.PICKEDUP;
        };

        // Pocketing animation finished. Give back control to player
        _eventReceiver.OnPocketingFinishedEvent += () => {
            _state = PickupState.NONE;
            _animator.SetInteger(ANIM_STATE, (int)AnimationStates.IDLE);
            _movementSystem.setPuppetMode(false);
            Destroy(_objectBeingPickedUp.gameObject);
            _objectBeingPickedUp = null;
        };

        // Unpocketing animation finished
        _eventReceiver.OnUnpocketingFinishedEvent += () => {
            _state = PickupState.INSPECTION;
            _animator.SetInteger(ANIM_STATE, (int)AnimationStates.INSPECT);
        };
    }

    void OnTriggerEnter(Collider other) {
        if (!IsOwner) { return; }

        // Sets target position to be 'stopRadius' away from the pickupable (in direction of player). Sets walking state
        if (other.gameObject.tag == "Pickupable" && _state == PickupState.NONE) {
            _movementSystem.setPuppetModeMoveTarget(other.transform.position + 
                                                (transform.position - other.transform.position).normalized * 
                                                other.GetComponent<Pickupable>().stopRadius);
            _movementSystem.setPuppetMode(true);
            _objectBeingPickedUp = other.gameObject.GetComponent<Pickupable>();
            _state = PickupState.WALKING;
        }
    }

    void Update() {
        // If state is after item picked up, have it follow hand
        if (_state >= PickupState.PICKEDUP) {
            _objectBeingPickedUp.ChangeTransformServerRpc(_handLocation.transform.position, _handLocation.transform.rotation);
        }
        Debug.Log(_animator.GetCurrentAnimatorClipInfo(0)[0].clip.name);
        Debug.Log(_animator.GetInteger(ANIM_STATE));
    }
}
