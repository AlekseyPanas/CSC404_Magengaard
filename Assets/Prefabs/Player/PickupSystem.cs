using System;
using Unity.Netcode;
using UnityEngine;


/** Manages the sequence of picking up an approached pickupable. Also exposes methods to "unpocket" items for animating the character
holding something if they "take out" the item they previously picked up */
public class PickupSystem: NetworkBehaviour {

    private static readonly int ANIM_STATE = Animator.StringToHash("AnimState");

    /** Notifies inspectables (or other objects) when the player is in puppet mode due to an in-progress inspection (i.e player can still be in puppet mode from another
    behavior, in which case this wont trigger). LIsteners can use this to prevent opening a second UI when one is already open by checking in another inspection is in progress */
    public static event Action<bool> OnChangePuppetModeDueToInspectionEvent = delegate { };

    /** When a new item is picked up with the pickup system and pocketed, this event is fired */
    public static event Action<Pickupable> OnInventoryAddEvent = delegate { };
    

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

    [Tooltip("The network prefabs list that contains all pickupables")]
    [SerializeField] private NetworkPrefabsList _pickupablesPrefabList;
    
    private Movement _movementSystem;  // Used to set puppet mode
    private PickupState _state = PickupState.NONE;  // State machine for transitioning through pickup sequence
    private Pickupable _objectBeingPickedUp = null;  // Tracks game object which is being held
    private bool _isUnpocketed = false;  // When true, means the _objectBeingPickedUp was spawned for unpocketing. Prevents adding the item to "inventory" twice
    private GameObject _inspParamTemp;  // Stores the inspectable gameobject passed for the OnUnpocketInspectableEvent to be used in the client rpc

    void Awake() {
        _movementSystem = GetComponent<Movement>();
        _movementSystem.arrivedAtTarget += () => {
            if (_state == PickupState.WALKING) { 
                _state = PickupState.PICKUP; 
                _animator.SetInteger(ANIM_STATE, (int)AnimationStates.PICKUP);
            }
        };

        // Once the full pickup sequence is finished
        _eventReceiver.OnFinishedPickupEvent += () => {
            _isUnpocketed = false;
            _SetInspect();
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
            
            // Right before destroying the associated pickupable, subscribe to the underlying inspectable's event to detect when the item is unpocketed again (add to inventory, effectively)
            if (_objectBeingPickedUp.Inspectable != null && !_isUnpocketed) { _objectBeingPickedUp.Inspectable.OnUnpocketInspectableEvent += (int networkPrefabListIndex, GameObject insp) => {
                if (_state != PickupState.NONE) { return; }

                _movementSystem.setPuppetModeMoveTarget(null);
                _movementSystem.setPuppetMode(true);
                OnChangePuppetModeDueToInspectionEvent(true);
                _inspParamTemp = insp;

                // Instantiates prefab server side. After finishing, the client RPC gets called to start the unpocketing animation
                NetworkSpawnPickupableServerRpc(_handLocation.position, _handLocation.rotation, networkPrefabListIndex, new ServerRpcParams());
            }; }

            _objectBeingPickedUp.NetworkDestroySelfServerRpc();
            _objectBeingPickedUp = null;
            OnChangePuppetModeDueToInspectionEvent(false);
        };

        // Unpocketing animation finished
        _eventReceiver.OnUnpocketingFinishedEvent += () => {
            _isUnpocketed = true;
            _SetInspect();
        };
    }

    /** Instantiates itself as the prefab and spawns it network side. Return the game object */
    [ServerRpc]
    public void NetworkSpawnPickupableServerRpc(Vector3 position, Quaternion orientation, int pickupableListIndex, ServerRpcParams clientDetails) {
        var obj = Instantiate(_pickupablesPrefabList.PrefabList[pickupableListIndex].Prefab, position, orientation);
        obj.GetComponent<NetworkObject>().Spawn();

        UnpocketItemSpawnedClientRpc(new NetworkObjectReference(obj), 
            new ClientRpcParams {
                Send = new ClientRpcSendParams {
                    TargetClientIds = new ulong[]{
                        clientDetails.Receive.SenderClientId
                    }
                }
            }
        );
    }

    // This is a continuation of execution from the OnUnpocketInspectableEvent. Prefab is spawned server side and this function is called after the prefab is spawned
    [ClientRpc]
    public void UnpocketItemSpawnedClientRpc(NetworkObjectReference spawnedPrefab, ClientRpcParams clientRpcParams) {
        NetworkObject spawnedPrefabNO;
        spawnedPrefab.TryGet(out spawnedPrefabNO);
        _objectBeingPickedUp = spawnedPrefabNO.gameObject.GetComponent<Pickupable>();
        _objectBeingPickedUp.SetInspectableGameObject(_inspParamTemp);

        _animator.SetInteger(ANIM_STATE, (int)AnimationStates.UNPOCKET);
        _state = PickupState.UNPOCKETING;
    } 

    void OnTriggerEnter(Collider other) {
        if (!IsOwner || _state != PickupState.NONE) { return; }

        // Sets target position to be 'stopRadius' away from the pickupable (in direction of player). Sets walking state
        if (other.gameObject.tag == "Pickupable" && _state == PickupState.NONE) {
            _movementSystem.setPuppetModeMoveTarget(other.transform.position + 
                                                (transform.position - other.transform.position).normalized * 
                                                other.GetComponent<Pickupable>().stopRadius);
            _movementSystem.setPuppetMode(true);
            _objectBeingPickedUp = other.gameObject.GetComponent<Pickupable>();
            _state = PickupState.WALKING;
            OnChangePuppetModeDueToInspectionEvent(true);
        }
    }

    /** Sets state to inspection and registers with the inspectable if not null */
    private void _SetInspect() {
        var insp = _objectBeingPickedUp.Inspectable;
        if (insp == null) { 
            _animator.SetInteger(ANIM_STATE, (int)AnimationStates.POCKET); 
            _state = PickupState.POCKETING;
        }
        else {
            _animator.SetInteger(ANIM_STATE, (int)AnimationStates.INSPECT);
            _state = PickupState.INSPECTION;

            insp.OnInspectStart(() => {
                _animator.SetInteger(ANIM_STATE, (int)AnimationStates.POCKET);
                _state = PickupState.POCKETING;
            });
        }
    }

    void Update() {
        // If state is after item picked up, have it follow hand
        if (_state >= PickupState.PICKEDUP) {
            _objectBeingPickedUp.ChangeTransformServerRpc(_handLocation.transform.position, _handLocation.transform.rotation);
        }
    }

    /** Call this method to make the character take an item out of their pocket and stop moving until corresponding IInspectable is closed */
    public void TakeOutItem(Pickupable prefab) {
        
    }
}
