using UnityEngine;

/**
 * Moves a camera position to the value given by an ICameraFollow.
 *
 * Manages ICameraFollow objects.
 */
public class CameraManager : MonoBehaviour
{
    private ICameraFollow _follow;
    private Object _holder;

    private ICameraFollow _overrideFollow;
    
    private CameraPosition _predecessor = CameraPosition.Zero;
    private float _elapsedTime = 0.0f;
    
    private Transform _followTransform;

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
    public void SwitchFollow(Object holder, ICameraFollow follow)
    {
        if (_holder != null && holder != null && ReferenceEquals(_holder, holder))
        {
            return;
        }
        
        _holder = holder;
        _follow = follow;
        
        ResetFollow();
    }

    public void AddOverrideFollow(ICameraFollow follow)
    {
        _overrideFollow = follow;

        _predecessor = Current;
        _elapsedTime = 0.0f;
        
        ResetFollow();
    }
    
    public void ClearOverrideFollow()
    {
        _overrideFollow = null;
        
        ResetFollow();
    }

    void Update()
    {
        _elapsedTime += Time.deltaTime;
        
        var follow = _overrideFollow ?? _follow;
        var position = follow.FollowPosition(Context);

        var local = transform;

        local.position = position.Position;
        local.forward = position.Forward;
    }
}
