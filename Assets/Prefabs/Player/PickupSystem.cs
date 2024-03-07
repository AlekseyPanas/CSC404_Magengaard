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

    /** Notifies inspectables (or other objects) when a new pickup sequence starts or finishes */
    public static event Action<bool> OnTogglePickupSequence = delegate { };

    /** When a new item is picked up with the pickup system and pocketed, this event is fired */
    public static event Action<Pickupable> OnInventoryAddEvent = delegate { };
    

    private enum PickupState {
        NONE = 0,
        WALKING = 1,
        PICKUP = 2,
        PICKEDUP = 3,
        INSPECTION = 4,
        POCKETING = 5,
        UNPOCKETING = 6,
        AWAITING_RPC = 7
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

    /** 
    * Destroys object being picked up on server side if not null and sets variable to null
    */
    void DestroyCurrentObjectBeingPickedUp() {
        if (_objectBeingPickedUp != null) { 
            _objectBeingPickedUp.NetworkDestroySelfServerRpc(); 
            _objectBeingPickedUp = null;
        }
    }

    public override ControllerRegistrant RegisterDefault() { return null; }

    // Only allows registration if no part of the pickup sequence is in progress
    public override ControllerRegistrant RegisterController(int priority) {
        if (_currentController != null || _state != PickupState.NONE || !TryGetControl()) { return null; }

        _currentPriority = int.MinValue;
        return base.RegisterController(priority);
    }

    public override void DeRegisterController(ControllerRegistrant controller) {
        if (_currentController == controller && _state == PickupState.INSPECTION) { OnInspectFinished(); }
        base.DeRegisterController(controller);
    }

    /** 
    Called either of the 2 systems is interrupted, cancel current pickup in progress 
    */
    void OnInterrupt() {
        _currentController.OnInterrupt();
        _currentController = null;

        DestroyCurrentObjectBeingPickedUp();
        
        if (_state != PickupState.NONE) {
            _state = PickupState.NONE; 
            _animator.SetInteger(ANIM_STATE, (int)AnimationStates.IDLE);
        }
        DeRegisterAll();
    }

    /** Frees controllables used for pickup */
    void DeRegisterAll() {
        _gestureControllable.DeRegisterController(_gestRegistrant);
        _movementControllable.DeRegisterController(_moveRegistrant);
    }

    /** 
    Tries to acquire control on the two core systems. Returns true if succeeded. Also subscribes to interrupts and other events
    */
    bool TryGetControl() {
        _moveRegistrant = _movementControllable.RegisterController((int)MovementControllablePriorities.PICKUP);
        _gestRegistrant = _gestureControllable.RegisterController((int)GestureControllablePriorities.PICKUP);
        if (_moveRegistrant == null || _gestRegistrant == null) {
            DeRegisterAll();
            return false;
        }
        _moveRegistrant.OnInterrupt += OnInterrupt;
        _moveRegistrant.OnArrivedTarget += OnArrivedAtPickupable;
        _gestRegistrant.OnInterrupt += OnInterrupt;

        _animator = _movementControllable.GetSystem(_moveRegistrant).GetAnimator();
        return true;
    }

    // We must guarantee at all times that if current controller is not null, then we have control of underlying systems
    // Furthermore, we must guarantee that a phase only executes if we still have control of underlying systems
    // We must also guarantee that during the execution of any phase, no new controller can register
    /**
    █▀▄ █▄█ ▄▀▄ ▄▀▀ ██▀   ▄█
    █▀  █ █ █▀█ ▄██ █▄▄    █
    Detect when collided with a pickupable and initiate walking sequence
    */
    void OnTriggerEnter(Collider other) {
        if (!IsOwner || _state != PickupState.NONE) { return; }
        if (other.gameObject.tag != "Pickupable") { return; }
        if (_currentController != null) { return; }
        if (!TryGetControl()) { return; }

        _objectBeingPickedUp = other.GetComponent<Pickupable>();
        _movementControllable.GetSystem(_moveRegistrant).MoveTo(other.transform.position, 2f, _objectBeingPickedUp.stopRadius);
        _state = PickupState.WALKING;
        OnTogglePickupSequence(true);
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
    void OnItemPickedUp() { 
        if (_state == PickupState.PICKUP) {
            _state = PickupState.PICKEDUP; 
        }
    }

    void Update() {
        // If state is after item picked up, have it follow hand
        if (_state >= PickupState.PICKEDUP) {
            _objectBeingPickedUp.ChangeTransformServerRpc(_handLocation.transform.position, _handLocation.transform.rotation);
        }
    }

    /**
    █▀▄ █▄█ ▄▀▄ ▄▀▀ ██▀   █▄█
    █▀  █ █ █▀█ ▄██ █▄▄     █
    Called once pickup animation finished. Changes state to inspection and notifies the associated UI
    */
    void OnFinishedPickup() {
        if (_state == PickupState.PICKEDUP) {
            _isUnpocketed = false;
            _SetInspect();
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

            if (!_isUnpocketed) { RegisterController(1); }
            insp.OnInspectStart(_currentController);
        }
    }

    /**
    █▀▄ █▄█ ▄▀▄ ▄▀▀ ██▀   ██▀
    █▀  █ █ █▀█ ▄██ █▄▄   ▄▄█
    Called when an open UI was closed (via deregistering). Triggers pocketing animation
    */
    void OnInspectFinished() {
        // OnInspectFinished got called => DeRegister got called by a controller => the controller equaled the current controler =>
        // OnInterrupt could not have been called since it sets current controller to null => we still have control of underlying systems
        _animator.SetInteger(ANIM_STATE, (int)AnimationStates.POCKET);
        _state = PickupState.POCKETING;
    } 

    /**
    █▀▄ █▄█ ▄▀▄ ▄▀▀ ██▀   █▀▀
    █▀  █ █ █▀█ ▄██ █▄▄   ███
    Called when an item was successfully pocketed, deregisters all systems since sequence finished
    */
    void OnPocketingFinished() {
        if (_state == PickupState.POCKETING) {
            _state = PickupState.NONE;
            _animator.SetInteger(ANIM_STATE, (int)AnimationStates.IDLE);
            DeRegisterAll();

            DestroyCurrentObjectBeingPickedUp();

            OnTogglePickupSequence(false);
        }
    }

    /**
    █▀▄ █▄█ ▄▀▄ ▄▀▀ ██▀   ▀▀█
    █▀  █ █ █▀█ ▄██ █▄▄    █
    Controller can call this after registering to initiate unpocketing of UI
    */
    public void StartUnpocketing(int pickupablesListIndex, GameObject inspectable) {
        // If this method is called => a registered controller called it => TryGetControl returned true => we have control of underlying systems
        if (_state != PickupState.NONE) { return; }

        OnTogglePickupSequence(true);
        _inspParamTemp = inspectable;
        _state = PickupState.AWAITING_RPC;

        // Instantiates prefab server side. After finishing, the client RPC gets called to start the unpocketing animation
        NetworkSpawnPickupableServerRpc(_handLocation.position, _handLocation.rotation, pickupablesListIndex, new ServerRpcParams());
    }

    
    [ServerRpc]  // Instantiates itself as the prefab and spawns it network side. Return the game object
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

    [ClientRpc]  // This is a continuation of execution from the OnUnpocketInspectableEvent. Prefab is spawned server side and this function is called after the prefab is spawned
    public void UnpocketItemSpawnedClientRpc(NetworkObjectReference spawnedPrefab, ClientRpcParams clientRpcParams) {
        NetworkObject spawnedPrefabNO;
        spawnedPrefab.TryGet(out spawnedPrefabNO);
        _objectBeingPickedUp = spawnedPrefabNO.gameObject.GetComponent<Pickupable>();
        _objectBeingPickedUp.SetInspectableGameObject(_inspParamTemp);

        if (_state == PickupState.AWAITING_RPC) {
            _animator.SetInteger(ANIM_STATE, (int)AnimationStates.UNPOCKET);
            _state = PickupState.UNPOCKETING;
        }

        // State was not consistent with sequence, meaning an interrupt happened. Destroy the newly spawned object since
        // the passed interrupt failed to destroy it
        else {
            DestroyCurrentObjectBeingPickedUp();
        }
            
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

    protected override PickupSystem ReturnSelf() { return this; }
}
