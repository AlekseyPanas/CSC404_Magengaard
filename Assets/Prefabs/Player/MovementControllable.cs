using System;
using Unity.Netcode;
using UnityEngine;

public class MovementControllable : AControllable<MovementControllable>
{    
    public Animator animator;

    public Transform headRayOrigin;
    public Transform feetRayOrigin;
    
    private float _hopWalkTime;
    private Vector2? _hopDirectionLock;  // Prevents midair strafing. A hop locks your direction to this vector until you are grounded again
    
    private CharacterController _controller;

    public float gravity = -9.81f;  // Gravity acceleration while airborne (due to hop)
    public float speedUserMovement = 4.0f;   // Speed while user is moving
    public float turnSpeed = 0.15f;
    public float turnAroundSpeed = 0.6f;

    public Vector2 lastInput;

    private Camera _activeCamera;
    
    private readonly NetworkVariable<Vector3> _velocity = new(
        new Vector3(), NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    
    // Animator Variables
    private static readonly int ANIM_STATE = Animator.StringToHash("AnimState");
    private static readonly int ANIM_SPEED = Animator.StringToHash("Speed");

    protected override MovementControllable ReturnSelf() { return this; }

    private void Awake() {
        _activeCamera = Camera.main;
    }

}