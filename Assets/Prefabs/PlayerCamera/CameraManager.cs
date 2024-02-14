using UnityEngine;


public class CameraManager : MonoBehaviour
{
    private ICameraFollow _follow;
    private Object _holder;
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

    public void SwitchFollow(Object holder, ICameraFollow follow)
    {
        if (ReferenceEquals(_holder, holder))
        {
            return;
        }
        
        _holder = holder;
        _follow = follow;
        _predecessor = Current;
        _elapsedTime = 0.0f;
    }

    void Update()
    {
        _elapsedTime += Time.deltaTime;
        
        var position = _follow.FollowPosition(Context);

        var local = transform;

        local.position = position.Position;
        local.forward = position.Forward;
    }
}
