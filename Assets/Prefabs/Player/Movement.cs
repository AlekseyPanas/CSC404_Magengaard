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
    public float speedUserMovement = 4.0f;   // Speed while user is moving
    public float speedPuppetMode = 2.0f;  // Speed while in puppet mode
    public float turnSpeed = 0.15f;
    public float turnAroundSpeed = 0.6f;

    public Vector2 lastInput;

    private Camera _activeCamera;
    
    private DesktopControls _controls;
    
    private readonly NetworkVariable<Vector3> _velocity = new(
        new Vector3(), NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    
    // Animator Variables
    private static readonly int ANIM_STATE = Animator.StringToHash("AnimState");
    private static readonly int ANIM_SPEED = Animator.StringToHash("Speed");

    private void Awake() {
        _controller = GetComponent<CharacterController>();

        _controls = new DesktopControls();
        _controls.Enable();
        _controls.Game.Enable();

        _activeCamera = Camera.main;
        PlayerHealthSystem.onDeath += OnDeath;
        PlayerHealthSystem.onRespawnFinished += OnRespawnFinished;
    }

    /** Given a Vector2 v representing horizontal motion where v.y is camera.forward and v.x is camera.right/left, return a Vector2
    in terms of pure world coordinates */
    private Vector2 horizontalCam2World(Vector2 camDirections) {
        var cameraForward = _activeCamera.transform.forward.normalized;
        var forward = new Vector3(cameraForward.x, 0, cameraForward.z);
        var right = new Vector3(cameraForward.z, 0, -cameraForward.x);
        var result = forward * camDirections.y + right * camDirections.x;
        return new Vector2(result.x, result.z);
    }

    private void UpdateVelocity() {
        if (!IsOwner) { return; }

        lastInput = _controls.Game.Movement.ReadValue<Vector2>();
        Vector2 horizontal;
        Vector3 velocity = _velocity.Value;  // Retrieves current velocity
        float vertical = velocity.y + gravity * Time.deltaTime;
        if (_controller.isGrounded) { // Cancels hopping lock if on ground
            _hopDirectionLock = null;
            vertical = -1.0f;
        }
        Vector2? diffToPuppetTarget = null;

        // Hopping takes priority. Midair movement cant be changed
        if (_hopDirectionLock.HasValue) {  
            horizontal = horizontalCam2World(_hopDirectionLock.Value);
            horizontal = horizontal.normalized * speedUserMovement;
        } 
        
        // Puppet mode moves player to preconfigured target (or stand still if target is null) 
        else if (_isPuppetMode) {  
            if (_puppetMoveTarget == null) { horizontal = Vector2.zero; }
            else { 
                var diff = _puppetMoveTarget - transform.position;
                diffToPuppetTarget = new Vector2(diff.Value.x, diff.Value.z);

                horizontal = diffToPuppetTarget.Value; 
            }
            horizontal = horizontal.normalized * speedPuppetMode;  
        } 
        
        // User input movement
        else {  
            horizontal = horizontalCam2World(lastInput);  // Reads in user input on movement axes
            horizontal = horizontal.normalized * speedUserMovement;
        }

        // Reverts speed to be the precise distance to target if it would overshoot and fires event to notify that player arrived at target
        if (diffToPuppetTarget != null && horizontal.sqrMagnitude * Time.deltaTime > diffToPuppetTarget.Value.sqrMagnitude) {
            horizontal = diffToPuppetTarget.Value / Time.deltaTime;
            arrivedAtTarget();
        }
        
        // Adjusts animation speed to be proportional to horizontal velocity (8 is a magic number, extract to constant?)
        animator.SetFloat(ANIM_SPEED, horizontal.sqrMagnitude / 4);  
        if (horizontal.sqrMagnitude > 0) { 
            if (!_isPuppetMode || animator.GetInteger(ANIM_STATE) == (int)AnimationStates.IDLE) {
                animator.SetInteger(ANIM_STATE, (int)AnimationStates.WALK); 
            }
        } else {
            if (!_isPuppetMode || animator.GetInteger(ANIM_STATE) == (int)AnimationStates.WALK) {
                animator.SetInteger(ANIM_STATE, (int)AnimationStates.IDLE); 
            } 
        }

        // Update velocity
        velocity = new Vector3(horizontal.x, 0, horizontal.y) + Vector3.up * vertical;
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

        var current = transform;
        
        if (velocity.sqrMagnitude > 0)
        {
            var turn = turnSpeed;
            
            if ((current.forward.normalized - velocity.normalized).magnitude < 0.03f)
            {
                turn = turnAroundSpeed;
            }

            var forward = Vector3.Lerp(current.forward, -velocity.normalized, turn);

            current.forward = forward;
        }
        
        // Zero Y Forward
        // For whatever reason, JJ spawns with a non-zero y forward. I don't know why.
        // I've done everything I can to normalize it.
        
        var direction = current.forward;
        
        current.forward = new Vector3(direction.x, 0, direction.z).normalized;
    }

    /** Toggle manual movement control vs player input movement control */
    public void setPuppetMode(bool isPuppetMode) { _isPuppetMode = isPuppetMode; }

    /** Set location to move to in puppet mode. Null cancels movement*/
    public void setPuppetModeMoveTarget(Vector3? target) {
        _puppetMoveTarget = target;
    }

    void OnDeath(){
        setPuppetMode(true);
    }

    void OnRespawnFinished(){
        setPuppetMode(false);
    }

    void OnDestroy(){
        PlayerHealthSystem.onDeath -= OnDeath;
        PlayerHealthSystem.onRespawnFinished -= OnRespawnFinished;
    }
}
