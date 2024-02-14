using System.Linq.Expressions;
using Unity.Netcode;
using UnityEngine;


public class Movement : NetworkBehaviour
{
    public Animator animator;
    
    private CharacterController _controller;

    public float gravity = -9.81f;
    public float speed = 2.0f;

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
    
        var velocity = _velocity.Value;
        var vertical = velocity.y + gravity * Time.deltaTime;
        var horizontal = _controls.Game.Movement.ReadValue<Vector2>() * speed;
        
        if (_controller.isGrounded) {
            vertical = -1.0f;
        }

        var cameraForward = _activeCamera.transform.forward;
        var forward = new Vector3(cameraForward.x, 0, cameraForward.z);
        var right = new Vector3(cameraForward.z, 0, -cameraForward.x);
        
        animator.SetBool(Walking, horizontal.sqrMagnitude > 0);

        velocity = forward * horizontal.y + right * horizontal.x + Vector3.up * vertical;
        
        _velocity.Value = velocity;
    }

    private void Update()
    {
        UpdateVelocity();

        var velocity = _velocity.Value;

        _controller.Move(velocity * Time.deltaTime);
        
        velocity.y = 0;

        if (velocity.sqrMagnitude > 0) {
            transform.forward = -velocity.normalized;
        }
    }
}
