using System.Collections.Generic;
using UnityEngine;

public enum CameraControllablePriorities {
    REGION = 0,
    PICKUP = 1,
    DEATH = 2,
    CUTSCENE = 3
}

/**
 * Moves a camera position to the value given by an ICameraFollow.
 *
 * Manages ICameraFollow objects.
 */
public class CameraManager : AControllable<CameraManager, ControllerRegistrant>
{
    private ICameraFollow _follow;
    
    private CameraPosition _predecessor = CameraPosition.Zero;
    private float _elapsedTime = 0.0f;
    
    private Transform _followTransform;

    public HashSet<string> Flags = new();

    void Awake()
    {
        var local = transform;
        _follow = new CameraFollowFixed(local.position, local.forward, 0.0f);
        
        // Subscribe to receive info when the player spawns in
        PlayerSpawnedEvent.OwnPlayerSpawnedEvent += t => _followTransform = t;
    }

    private CameraPosition Current => new()
    {
        Position = transform.position,
        Forward = transform.forward
    };
    
    private CameraFollowContext Context =>
        new()
        {
            Follow = _followTransform != null ? _followTransform.position : null, // ?. is bad?
            Current = Current,
            Predecessor = _predecessor,
            TimeElapsed = _elapsedTime
        };

    private void ResetFollow()
    {
        _predecessor = Current;
        _elapsedTime = 0.0f;
    }

    /**
     * Sets the current ICameraFollow object.
     *
     * Holder is the origin object making this request.
     * If the current ICameraFollow has been put into place by this holder, then this call is ignored.
     * If holder is null, then the switch will be done regardless.
     */
    public void SwitchFollow(ControllerRegistrant holder, ICameraFollow follow)
    {
        if (_currentController != null && holder != null && !ReferenceEquals(_currentController, holder)) {
            return;
        }
        _follow = follow;
        
        ResetFollow();
    }

    public ICameraFollow GetCurrentFollow(ControllerRegistrant holder){
        if (_currentController != null && holder != null && !ReferenceEquals(_currentController, holder)) {
            return null;
        }
        return _follow;
    }

    public void JumpTo(CameraPosition position)
    {
        _predecessor = position;

        var current = transform;
        
        current.position = position.Position;
        current.forward = position.Forward;
    }

    void FixedUpdate()
    {
        _elapsedTime += Time.fixedDeltaTime;
        
        var position = _follow.FollowPosition(Context);

        // Prevent camera from going out of bounds
        if (_followTransform != null)
        {
            var start = _followTransform.position;
            var end = position.Position;

            var distance = (end - start);
            var direction = distance.normalized;
            var length = distance.magnitude;

            RaycastHit hit;
            if (Physics.Raycast(start, direction, out hit, length, 0b1000000))
            {
                Debug.Log("Hello " + hit.transform.gameObject.layer);
                
                position.Position = hit.point - direction * 0.1f;
            }
        }

        var local = transform;

        local.position = position.Position;
        local.forward = position.Forward;
        
        
    }

    protected override CameraManager ReturnSelf() { return this; }
}
