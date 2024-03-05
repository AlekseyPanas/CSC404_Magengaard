using System;
using Unity.Netcode;
using UnityEngine;

/** Controllable which exposes player movement and animation functionality */
public class MovementControllable : AControllable<MovementControllable> {    
    public static readonly float PLAYER_GRAVITY_ACCEL = -9.81f;  // Gravity acceleration while airborne (due to hop or falling)

    [SerializeField] private Animator _animator;
    [SerializeField] private Transform _headRayOrigin;
    [SerializeField] private Transform _feetRayOrigin;
    [SerializeField] private float _velocityMultiplierDistanceToTriggerHop = 3;  // If the player is going to move X distance in a frame, multiply X by this constant. If an obstacle is within the resulting distance, tries to trigger hop if possible
    private CharacterController _controller;
    private Camera _activeCamera;
    private Vector2? _hopDirectionLock;  // Prevents midair strafing. A hop locks your direction to this vector until you are grounded again
    
    // Animator Variables
    private static readonly int ANIM_STATE = Animator.StringToHash("AnimState");
    private static readonly int ANIM_SPEED = Animator.StringToHash("Speed");

    private void Awake() {
        _controller = GetComponent<CharacterController>();
        _activeCamera = Camera.main;
    }

    public event Action OnArrivedTarget = delegate { };

    /** Move to a given location automatically with the given speed. Stop once within the provided radius of the target. 
    * Hop over obstacles if necessary. Fire arrived event when done */
    public void MoveTo(Vector3 target, float speed, float stopRadius) {

    }

    /** Move in a given direction with a speed (velocity). Hop over obstacles if necessary. 
    * Cancels target movement in progress. The vector2 is provided relative to camera forward (x) and camera right (y) */
    public void MoveDir(Vector2 direction, float speed) {
        _controller.Move(direction.normalized * speed * Time.deltaTime);
    }

    /** Stops any motion in progress (interrupts MoveTo) */
    public void Stop() {

    }

    /** Acquire the animator component connected to the player */
    public Animator GetAnimator() { return _animator; }

    // Reset internal state when a new controller takes over
    protected override void OnControllerChange() {

    }

    protected override MovementControllable ReturnSelf() { return this; }
    
    // Manages walking to target and hopping
    private void Update() {

    }

    // Given a world coordinate velocity (magnitude and direction), return if the player should hop
    private bool _ShouldHop(Vector3 velocity) {
        var feetHit = Physics.Raycast(_feetRayOrigin.position, -transform.forward, out hit, 1, withoutPlayer);
    }
}