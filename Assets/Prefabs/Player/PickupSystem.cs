using AMovementControllable = AControllable<MovementControllable, MovementControllerRegistrant>;
using AGestureControllable = AControllable<AGestureSystem, GestureSystemControllerRegistrant>;
using ACameraControllable = AControllable<CameraManager, ControllerRegistrant>;
using System;
using Unity.Netcode;
using UnityEngine;


/** Manages the sequence of picking up an approached pickupable. Also exposes methods to "unpocket" items for animating the character
holding something if they "take out" the item they previously picked up */
public class PickupSystem: AControllable<PickupSystem, ControllerRegistrant> {

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

    private Animator _animator;  // Animator of the child fbx model

    [Tooltip("Event receiver script attached to child fbx mode (same as animator)")]
    [SerializeField] private AnimationEventReceiver _eventReceiver;
    
    [Tooltip("object within the rig which represents the hand that will hold the picked up item")]
    [SerializeField] private Transform _handLocation;

    [Tooltip("The network prefabs list that contains all pickupables")]
    [SerializeField] private NetworkPrefabsList _pickupablesPrefabList;
    
    // The 2 core controllables needed to execute a pickup sequence
    [Tooltip("The gesture system")] [SerializeField] private AGestureControllable _gestureControllable;
    private AMovementControllable _movementControllable;
    private GestureSystemControllerRegistrant _gestRegistrant;
    private MovementControllerRegistrant _moveRegistrant;

    private PickupState _state = PickupState.NONE;  // State machine for transitioning through pickup sequence
    private Pickupable _objectBeingPickedUp = null;  // Tracks game object which is being held
    private bool _isUnpocketed = false;  // When true, means the _objectBeingPickedUp was spawned for unpocketing. Prevents adding the item to "inventory" twice
    private GameObject _inspParamTemp;  // Stores the inspectable gameobject passed for the OnUnpocketInspectableEvent to be used in the client rpc

    public override ControllerRegistrant RegisterDefault() { return null; }

    // Only allows registration if no other UI is registered
    public override ControllerRegistrant RegisterController(int priority) {
        if (_currentController == null) { 
            _currentPriority = int.MinValue;
            return base.RegisterController(priority);
        } return null;
    }

    public override void DeRegisterController(ControllerRegistrant controller) {
        if (_currentController == controller) { OnInspectFinished(); }
        base.DeRegisterController(controller);
    }

    /** 
    Controller can call this after registering to initiate unpocketing of UI
    */
    public void StartUnpocketing(int pickupablesListIndex, GameObject inspectable) {
        if (_state != PickupState.NONE) { return; }

        _movementSystem.setPuppetModeMoveTarget(null);
        _movementSystem.setPuppetMode(true);
        OnChangePuppetModeDueToInspectionEvent(true);
        _inspParamTemp = insp;

        // Instantiates prefab server side. After finishing, the client RPC gets called to start the unpocketing animation
        NetworkSpawnPickupableServerRpc(_handLocation.position, _handLocation.rotation, networkPrefabListIndex, new ServerRpcParams());
    }

    /** 
    Called either of the 2 systems is interrupted, cancel current pickup in progress 
    */
    void OnInterrupt() {

    }

    /** 
    Tries to acquire control on the two core systems. Returns true if succeeded. Also subscribes to interrupts and other events
    */
    bool TryGetControl() {
        _moveRegistrant = _movementControllable.RegisterController((int)MovementControllablePriorities.PICKUP);
        _gestRegistrant = _gestureControllable.RegisterController((int)GestureControllablePriorities.PICKUP);
        if (_moveRegistrant == null || _gestRegistrant == null) {
            _gestureControllable.DeRegisterController(_gestRegistrant);
            _movementControllable.DeRegisterController(_moveRegistrant);
            return false;
        }
        _moveRegistrant.OnInterrupt += OnInterrupt;
        _moveRegistrant.OnArrivedTarget += OnArrivedAtPickupable;
        _gestRegistrant.OnInterrupt += OnInterrupt;
        return true;
    }

    /**
    █▀▄ █▄█ ▄▀▄ ▄▀▀ ██▀   ▄█
    █▀  █ █ █▀█ ▄██ █▄▄    █
    Detect when collided with a pickupable and initiate walking sequence
    */
    void OnTriggerEnter(Collider other) {
        if (!IsOwner || _state != PickupState.NONE) { return; }
        if (other.gameObject.tag != "Pickupable") { return; }
        if (!TryGetControl()) { return; }

        _objectBeingPickedUp = other.GetComponent<Pickupable>();
        _movementControllable.GetSystem(_moveRegistrant).MoveTo(other.transform.position, 2f, _objectBeingPickedUp.stopRadius);
        _state = PickupState.WALKING;
    }

    /**
    █▀▄ █▄█ ▄▀▄ ▄▀▀ ██▀   ▀▀█
    █▀  █ █ █▀█ ▄██ █▄▄   ██▄
    Called once arrived at pickupable. Next initiate crouch and pickup animation
    */
    void OnArrivedAtPickupable() {
        if (_state == PickupState.WALKING) { 
            _state = PickupState.PICKUP; 
            _animator.SetInteger(ANIM_STATE, (int)AnimationStates.PICKUP);
        }
    }

    /**
    █▀▄ █▄█ ▄▀▄ ▄▀▀ ██▀   ▀██
    █▀  █ █ █▀█ ▄██ █▄▄   ▄▄█
    Called during the phase of the pickup animation where JJ's hand touches the ground (i.e item picked up). Changes state so that pickupable now moves with hand
    */
    void OnItemPickedUp() { _state = PickupState.PICKEDUP; }

    /**
    █▀▄ █▄█ ▄▀▄ ▄▀▀ ██▀   █▄█
    █▀  █ █ █▀█ ▄██ █▄▄     █
    Called once pickup animation finished. Changes state to inspection and notifies the associated UI
    */
    void OnFinishedPickup() {
        _isUnpocketed = false;
        _SetInspect();
    }

    /**
    █▀▄ █▄█ ▄▀▄ ▄▀▀ ██▀   ██▀
    █▀  █ █ █▀█ ▄██ █▄▄   ▄▄█
    Called when an open UI was closed. Triggers pocketing animation
    */
    void OnInspectFinished() {
        _animator.SetInteger(ANIM_STATE, (int)AnimationStates.POCKET);
        _state = PickupState.POCKETING;
    } 

    /**
    █▀▄ █▄█ ▄▀▄ ▄▀▀ ██▀   █▀▀
    █▀  █ █ █▀█ ▄██ █▄▄   ███
    Called when an item was successfully pocketed, deregisters all systems since sequence finished
    */
    void OnPocketingFinished() {
        _state = PickupState.NONE;
        _animator.SetInteger(ANIM_STATE, (int)AnimationStates.IDLE);
        _movementSystem.setPuppetMode(false);
        _movementSystem.setPuppetModeMoveTarget(null);

        _objectBeingPickedUp.NetworkDestroySelfServerRpc();
        _objectBeingPickedUp = null;
        OnChangePuppetModeDueToInspectionEvent(false);
    }

    /**
    █▀▄ █▄█ ▄▀▄ ▄▀▀ ██▀   ▀▀█
    █▀  █ █ █▀█ ▄██ █▄▄    █
    When a UI successfully triggered an unpocketing event. Start unpocketing animation
    */
    void OnUnpocketingTriggered() {

    }

    /**
    █▀▄ █▄█ ▄▀▄ ▄▀▀ ██▀   █▄█
    █▀  █ █ █▀█ ▄██ █▄▄   █▄█
    When unpocketing finished, move to inspection
    */
    void OnUnpocketingFinished() {
        _isUnpocketed = true;
        _SetInspect();
    }

    /** 
    Subscribe phase events 
    */
    void Awake() {
        _movementControllable = GetComponent<AMovementControllable>();
        
        _eventReceiver.OnFinishedPickupEvent += OnFinishedPickup;
        _eventReceiver.OnItemPickedUpEvent += OnItemPickedUp;
        _eventReceiver.OnPocketingFinishedEvent += OnPocketingFinished;
        _eventReceiver.OnUnpocketingFinishedEvent += OnUnpocketingFinished;
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

            insp.OnInspectStart(OnInspectFinished);
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

    protected override PickupSystem ReturnSelf() { return this; }
}
