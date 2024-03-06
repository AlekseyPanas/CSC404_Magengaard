using AMovementControllable = AControllable<MovementControllable, MovementControllerRegistrant>;
using System;
using Unity.Netcode;
using UnityEngine;


/** The default controller for the movement controllable. Reads in user input from WASD keys and calls appropriate
methods to move the player */
public class MovementUserController : NetworkBehaviour {
    [SerializeField] private float speed = 4.0f;   // Speed while user is moving
    private AMovementControllable _moveSys;
    private MovementControllerRegistrant _moveSysDefaultRegistrant;
    private DesktopControls _controls;
    private bool _isResumed = true;  // Whether this controller is currently controlling the movement system

    private void Awake() {
        _controls = new DesktopControls();
        _controls.Enable();
        _controls.Game.Enable();

        _moveSys = GetComponent<AMovementControllable>();
    }

    private void Start() {
        _moveSysDefaultRegistrant = _moveSys.RegisterDefault();
        _moveSysDefaultRegistrant.OnResume += OnResume;
        _moveSysDefaultRegistrant.OnInterrupt += OnInterrupt;
    }

    private void OnResume() { _isResumed = true; }

    private void OnInterrupt() { _isResumed = false; }

    private void Update() {
        if (!IsOwner || !_isResumed) { return; }

        _moveSys.GetSystem(_moveSysDefaultRegistrant).MoveDir(_controls.Game.Movement.ReadValue<Vector2>(), speed);
    }
}
