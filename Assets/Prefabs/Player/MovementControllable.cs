using System;
using Unity.Netcode;
using UnityEngine;


public enum MovementControllablePriorities {
    PICKUP = 1,
    DEATH = 2,
    CUTSCENE = 3
}


public class MovementControllerRegistrant: ControllerRegistrant {
    public Action OnArrivedTarget = delegate { };  // When movement system MoveTo arrives at target
} 


/** Controllable which exposes player movement and animation functionality */
public class MovementControllable : AControllable<MovementControllable, MovementControllerRegistrant> {

    public static readonly float PLAYER_GRAVITY_ACCEL = -9.81f;  // Gravity acceleration while airborne (due to hop or falling)

    [SerializeField] private Animator _animator;
    [SerializeField] private Transform _headRayOrigin;
    [SerializeField] private Transform _feetRayOrigin;
    [SerializeField] private float _velocityMultiplierDistanceToTriggerHop = 3;  // If the player is going to move X distance in a frame, multiply X by this constant. If an obstacle is within the resulting distance, tries to trigger hop if possible
    [SerializeField] private float _maxNormalYCoordToHop = 0.01f;  // If the obstacle's side face normal has a Y-coord above this value, JJ doesnt hop. In other words, hops up vertical obstacles but not slanted surfaces
    [SerializeField] private float _hopInitialVerticalSpeed = 7f;
    [SerializeField] private float _turnLerpSpeedFactor = 0.1f;  // Factor at which JJ's current look direction is lerped to the target one
    private CharacterController _controller;
    private Camera _activeCamera;
    private Vector3? _moveTarget = null;  // If not null, player automatically moves to this point
    private float _moveTargetSpeed = 0f;  // Speed of moving to the set target
    private float _moveTargetStopRadius = 0f;  // Radius within which to stop moving to the target
    private Vector2? _hopDirectionLock = null;  // Prevents midair strafing. A hop locks your direction to this vector until you are grounded again
    private Vector3 _velocity = new(0, 0, 0);  // Built up every frame, tracks current velocity

    // Animator Variables
    private static readonly int ANIM_STATE = Animator.StringToHash("AnimState");
    private static readonly int ANIM_SPEED = Animator.StringToHash("Speed");

    private void Awake() {
        _controller = GetComponent<CharacterController>();
        _activeCamera = Camera.main;
    }

    

    /** Move to a given location automatically with the given speed. Stop once within the provided radius of the target. 
    * Hop over obstacles if necessary. Fire arrived event when done */
    public void MoveTo(Vector3 target, float speed, float stopRadius) {
        _moveTarget = target;
        _moveTargetSpeed = speed;
        _moveTargetStopRadius = stopRadius;
    }

    /** Move in a given direction with a speed (velocity). Hop over obstacles if necessary. 
    * Cancels target movement in progress. The vector2 is provided relative to camera forward (direction.y) and camera right (direction.x) */
    public void MoveDir(Vector2 direction, float speed) {
        _moveTarget = null;  // Disables target movement

        Vector2 worldVel = _horizontalCam2World(direction).normalized * speed;  // compute horizontal world x,z move velocity

        if (_hopDirectionLock == null) {
            if (_ShouldHop(worldVel)) {
                _hopDirectionLock = worldVel;
                _velocity.y = _hopInitialVerticalSpeed;
            } else {
                _velocity.x = worldVel.x;
                _velocity.z = worldVel.y;
            }
        }   
    }

    /** Cancels the current MoveTo target if one was set */
    public void StopMoveTo() { _moveTarget = null; }

    /** Acquire the animator component connected to the player */
    public Animator GetAnimator() { return _animator; }

    /** Return the ID hash needed to change animation state with the animator */
    public int GetAnimStateHash() { return ANIM_STATE; }

    // Reset internal state when a new controller takes over
    protected override void OnControllerChange() { _moveTarget = null; }

    protected override MovementControllable ReturnSelf() { return this; }
    
    // Manages movement
    private void Update() {
        if (!IsOwner) { return; }

        // Apply gravity
        _velocity.y += PLAYER_GRAVITY_ACCEL * Time.deltaTime;  

        // Overrides velocity x,z with hop if hop is active
        if (_hopDirectionLock != null) {
            _velocity.x = ((Vector2)_hopDirectionLock).x;
            _velocity.z = ((Vector2)_hopDirectionLock).y;
        } 
        
        // Moves in direction of target, stops if within stop radius and fires event
        else if (_moveTarget != null) {
            Vector3 diff = (Vector3)(_moveTarget - transform.position);
            Vector2 hdiff = new Vector2(diff.x, diff.z);

            if (hdiff.magnitude <= _moveTargetStopRadius) {
                _moveTarget = null;
                _currentController.OnArrivedTarget();
            } else {
                MoveDir(hdiff, _moveTargetSpeed);
                // Resets velocity to not overshoot target (if it would otherwise)
                if (_hopDirectionLock != null && new Vector2(_velocity.x, _velocity.z).sqrMagnitude * Time.deltaTime > hdiff.sqrMagnitude) {
                    _velocity.x = hdiff.x / Time.deltaTime;
                    _velocity.z = hdiff.y / Time.deltaTime;
                }
            }
        }

        // Update look direction
        if (_velocity.sqrMagnitude > 0) {
            transform.forward = Vector3.Lerp(transform.forward, -_velocity.normalized, _turnLerpSpeedFactor);
        }
        transform.forward = new Vector3(transform.forward.x, 0, transform.forward.z).normalized;  // Explicitly zero the Y to prevent angled JJ

        // Move
        _controller.Move(_velocity * Time.deltaTime);

        // Animations
        Vector2 horizontal = new Vector2(_velocity.x, _velocity.z);
        _animator.SetFloat(ANIM_SPEED, horizontal.magnitude / 4);
        if (_animator.GetInteger(ANIM_STATE) == (int)AnimationStates.IDLE && horizontal.sqrMagnitude > 0) {
            _animator.SetInteger(ANIM_STATE, (int)AnimationStates.WALK); 
        } else if (_animator.GetInteger(ANIM_STATE) == (int)AnimationStates.WALK && horizontal.sqrMagnitude == 0) {
            _animator.SetInteger(ANIM_STATE, (int)AnimationStates.IDLE); 
        }

        // Stops hop lock if on ground
        if (_controller.isGrounded) {
            _hopDirectionLock = null;
            _velocity.y = -1.0f;
        } 
        // If falling, sets hop to prevent mid fall direction change
        else {
            _hopDirectionLock = new Vector2(_velocity.x, _velocity.z);
        }

        // Resets movement back to 0
        _velocity.x = 0f;
        _velocity.z = 0f;
    }

    // Given a world coordinate velocity (magnitude and direction), return if the player should hop. Intuitively, hops when head raycast is clear
    // but feet raycast hits something within some range. This means there is a short enough obstacle to jump over. Velocity is given as a Vector2
    // to exclude vertical
    private bool _ShouldHop(Vector2 velocity) {
        var withoutPlayer = ~(1 << 3);  // Layer mask to avoid colliding with the player
        RaycastHit hit;

        var feetHit = Physics.Raycast(_feetRayOrigin.position, velocity, out hit, velocity.magnitude * _velocityMultiplierDistanceToTriggerHop, withoutPlayer);
        if (!feetHit || Math.Abs(hit.normal.y) > _maxNormalYCoordToHop) { return false; }  // Prevents hopping on slanted surfaces (e.g slopes). Only vertical obstacles

        var headHit = Physics.Raycast(_headRayOrigin.position, velocity, velocity.magnitude * _velocityMultiplierDistanceToTriggerHop, withoutPlayer);
        if (!headHit) { return false; }

        return true;
    }

    // Given a Vector2 v representing horizontal motion where v.y is camera.forward and v.x is camera.right/left, return a Vector2(x,z) direction
    // in terms of pure world coordinates. The returned direction has a world space magnitude of one */
    private Vector2 _horizontalCam2World(Vector2 camDirections) {
        var cameraForward = _activeCamera.transform.forward;
        var forward = new Vector2(cameraForward.x, cameraForward.z);
        var right = new Vector2(cameraForward.z, -cameraForward.x);  // Negative reciprocal for orthogonal right vector
        var result = forward * camDirections.y + right * camDirections.x;
        return result.normalized;
    }
}


/**
// FORMER TURNAROUND CODE. In current version, removed the turnSpeed vs turnAroundSpeed entirely, if that causes bug we can revert
var turn = turnSpeed;
            
if ((current.forward.normalized - velocity.normalized).magnitude < 0.03f)
{
    turn = turnAroundSpeed;
}
*/

// TODO: Might need to bring back the velocity as a network variable so that every client can update the player on their end, but only the owner sets the velocity
