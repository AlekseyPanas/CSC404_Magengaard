using System;
using Unity.Netcode;
using UnityEngine;


public class Movement : NetworkBehaviour
{
    private bool _isPuppetMode = false;  // If true, user input disabled and player moves to target location set by _puppetMoveTarget
    private Vector3? _puppetMoveTarget = null;  // Target to where the player automatically moves when in puppet mode. Null means stand still
    public event Action arrivedAtTarget = delegate { };  // Fired when in puppet mode and the player has arrived at the target location

    public Animator animator;

    public Transform headRayOrigin;
    public Transform feetRayOrigin;
    
    private float _hopWalkTime;
    private Vector2? _hopDirectionLock;  // Prevents midair strafing. A hop locks your direction to this vector until you are grounded again
    
    private CharacterController _controller;

    public float gravity = -9.81f;  // Gravity acceleration while airborne (due to hop)
    public float speed = 4.0f;
    public float turnSpeed = 0.15f;
    public float turnAroundSpeed = 0.6f;

    public Vector2 lastInput;

    private Camera _activeCamera;
    
    private DesktopControls _controls;
    
    private readonly NetworkVariable<Vector3> _velocity = new(
        new Vector3(), NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    
    // Animator Variables
    private static readonly int AnimWalking = Animator.StringToHash("Walking");
    private static readonly int AnimSpeed = Animator.StringToHash("Speed");

    private void Awake() {
        _controller = GetComponent<CharacterController>();

        _controls = new DesktopControls();
        _controls.Enable();
        _controls.Game.Enable();

        _activeCamera = Camera.main;
    }

    private void UpdateVelocity() {
        if (!IsOwner) { return; }

        lastInput = _controls.Game.Movement.ReadValue<Vector2>();  // Reads in user input on movement axes
        Vector3 velocity = _velocity.Value;  // Retrieves current velocity

        float vertical = velocity.y + gravity * Time.deltaTime;
        Vector2 horizontal = lastInput; 

        // If in puppet mode, move toward target (or stand still if target is null)
        Vector2? diffToPuppetTarget = null;
        if (_isPuppetMode) {
            if (_puppetMoveTarget == null) { horizontal = Vector2.zero; }
            else { 
                var diff = _puppetMoveTarget - transform.position;
                diffToPuppetTarget = new Vector2(diff.Value.x, diff.Value.z);

                horizontal = diffToPuppetTarget.Value; 
            }
        }
        
        // Cancels hopping lock if on ground
        if (_controller.isGrounded) {
            _hopDirectionLock = null;
            vertical = -1.0f;
        }

        // Overrides horizontal motion with locked value if hop in progress
        if (_hopDirectionLock.HasValue) { horizontal = _hopDirectionLock.Value; }

        horizontal = horizontal.normalized * speed;  // Normalizes and applies speed

        // Reverts speed to be the precise distance to target if it would overshoot and fires event to notify that player arrived at target
        if (diffToPuppetTarget != null && horizontal.sqrMagnitude > diffToPuppetTarget.Value.sqrMagnitude) {
            horizontal = diffToPuppetTarget.Value;
            arrivedAtTarget();
        }

        var cameraForward = _activeCamera.transform.forward.normalized;
        var forward = new Vector3(cameraForward.x, 0, cameraForward.z);
        var right = new Vector3(cameraForward.z, 0, -cameraForward.x);
        
        animator.SetFloat(AnimSpeed, horizontal.sqrMagnitude / 8);  // Adjusts animation speed to be proportional to horizontal velocity (8 is a magic number, extract to constant?)
        animator.SetBool(AnimWalking, horizontal.sqrMagnitude > 0);

        velocity = forward * horizontal.y + right * horizontal.x + Vector3.up * vertical;
        
        _velocity.Value = velocity;
    }

    private void DoHop() {
        _velocity.Value += Vector3.up * 7;
        
        _hopWalkTime = 0.0f;
        _hopDirectionLock = lastInput;
    }

    private void UpdateHop() {
        var withoutPlayer = ~(1 << 3);

        if (lastInput.sqrMagnitude == 0.0) {
            _hopWalkTime = 0.0f;
            return;
        }

        RaycastHit hit;

        var feetHit = Physics.Raycast(feetRayOrigin.position, -transform.forward, out hit, 1, withoutPlayer);
        
        if (!feetHit || Math.Abs(hit.normal.y) > 0.01) {
            _hopWalkTime = 0.0f;
            return;
        }

        var headHit = Physics.Raycast(headRayOrigin.position, -transform.forward, out _, 1, withoutPlayer);

        if (headHit) {
            _hopWalkTime = 0.0f;
            return;
        }
        
        _hopWalkTime += Time.deltaTime;

        if (!(_hopWalkTime > 0.6f)) {
            return;
        }
        
        DoHop();
    }

    private void Update() {
        UpdateVelocity();
        UpdateHop();

        var velocity = _velocity.Value;

        _controller.Move(velocity * Time.deltaTime);
        
        velocity.y = 0;

        if (velocity.sqrMagnitude > 0) {
            var turn = turnSpeed;
            
            if ((transform.forward.normalized - velocity.normalized).magnitude < 0.03f) {
                turn = turnAroundSpeed;
            }
            
            transform.forward = Vector3.Lerp(transform.forward, -velocity.normalized, turn);
        }
    }

    /** Toggle manual movement control vs player input movement control */
    public void setPuppetMode(bool isPuppetMode) { _isPuppetMode = isPuppetMode; }

    /** Set location to move to in puppet mode. Null cancels movement*/
    public void setPupperModeMoveTarget(Vector3? target) {
        _puppetMoveTarget = target;
    }
}
