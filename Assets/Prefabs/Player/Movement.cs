using System;
using Unity.Netcode;
using UnityEngine;


public class Movement : NetworkBehaviour
{
    public float speedUserMovement = 4.0f;   // Speed while user is moving
    public float speedPuppetMode = 2.0f;  // Speed while in puppet mode
    
    public float turnSpeed = 0.15f;
    public float turnAroundSpeed = 0.6f;

    public Vector2 lastInput;
    
    private DesktopControls _controls;
    

    private void Awake() {

        _controls = new DesktopControls();
        _controls.Enable();
        _controls.Game.Enable();
    }

    private void UpdateVelocity() {
        if (!IsOwner) { return; }

        lastInput = _controls.Game.Movement.ReadValue<Vector2>();
    }
}
