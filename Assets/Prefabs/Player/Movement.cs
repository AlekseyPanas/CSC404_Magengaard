using System;
using System.Linq.Expressions;
using Unity.Netcode;
using UnityEngine;


public class Movement : NetworkBehaviour
{
    public Animator animator;

    public Transform headRayOrigin;
    public Transform feetRayOrigin;
    
    private float _hopWalkTime;
    private Vector3? _hopDirectionLock;
    
    private CharacterController _controller;

    public float gravity = -9.81f;
    public float speed = 2.0f;
    public float turnSpeed = 0.15f;
    public float turnAroundSpeed = 0.6f;

    public Vector3 lastInput;

    private Camera _activeCamera;
    
    private DesktopControls _controls;
    
    private readonly NetworkVariable<Vector3> _velocity = new(
        new Vector3(), NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    
    private static readonly int Walking = Animator.StringToHash("Walking");

    private void Awake()
    {
        _controller = GetComponent<CharacterController>();

        _controls = new DesktopControls();
        
        _controls.Enable();
        _controls.Game.Enable();

        _activeCamera = Camera.main;
    }

    private void UpdateVelocity()
    {
        if (!IsOwner) {
            return;
        }

        lastInput = _controls.Game.Movement.ReadValue<Vector2>();
    
        var velocity = _velocity.Value;
        var vertical = velocity.y + gravity * Time.deltaTime;
        
        if (_controller.isGrounded)
        {
            _hopDirectionLock = null;
            
            vertical = -1.0f;
        }
        
        var horizontal = lastInput;

        if (_hopDirectionLock.HasValue)
        {
            horizontal = _hopDirectionLock.Value;
        }

        horizontal *= speed;
        
        var cameraForward = _activeCamera.transform.forward;
        var forward = new Vector3(cameraForward.x, 0, cameraForward.z);
        var right = new Vector3(cameraForward.z, 0, -cameraForward.x);
        
        animator.SetBool(Walking, horizontal.sqrMagnitude > 0);

        velocity = forward * horizontal.y + right * horizontal.x + Vector3.up * vertical;
        
        _velocity.Value = velocity;
    }

    private void DoHop()
    {
        _velocity.Value += Vector3.up * 7;
        
        _hopWalkTime = 0.0f;
        _hopDirectionLock = lastInput;
    }

    private void UpdateHop()
    {
        var withoutPlayer = ~(1 << 3);

        if (lastInput.sqrMagnitude == 0.0)
        {
            _hopWalkTime = 0.0f;
            return;
        }

        RaycastHit hit;

        var feetHit = Physics.Raycast(feetRayOrigin.position, -transform.forward, out hit, 1, withoutPlayer);
        
        if (!feetHit || Math.Abs(hit.normal.y) > 0.01)
        {
            _hopWalkTime = 0.0f;
            return;
        }

        var headHit = Physics.Raycast(headRayOrigin.position, -transform.forward, out _, 1, withoutPlayer);

        if (headHit)
        {
            _hopWalkTime = 0.0f;
            return;
        }
        
        _hopWalkTime += Time.deltaTime;

        if (!(_hopWalkTime > 0.6f))
        {
            return;
        }
        
        DoHop();
    }

    private void Update()
    {
        UpdateVelocity();
        UpdateHop();

        var velocity = _velocity.Value;

        _controller.Move(velocity * Time.deltaTime);
        
        velocity.y = 0;

        if (velocity.sqrMagnitude > 0)
        {
            var turn = turnSpeed;
            
            if ((transform.forward.normalized - velocity.normalized).magnitude < 0.03f)
            {
                turn = turnAroundSpeed;
            }
            
            transform.forward = Vector3.Lerp(transform.forward, -velocity.normalized, turn);
        }
    }
}
