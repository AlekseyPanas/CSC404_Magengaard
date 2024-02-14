using UnityEngine;


public class FollowCamera : MonoBehaviour 
{
    private Transform _followTransform;

    public float followSpeed = 0.01f;
    public Vector3 followOffset;

    void Awake() {
        PlayerSpawnedEvent.OwnPlayerSpawnedEvent += Follow;  // Subscribe to receive info when the player spawns in
    }

    public void Follow(Transform follow)
    {
        
        _followTransform = follow;
    }

    void Update()
    {
        if (_followTransform != null) {
            var target = Vector3.Lerp(transform.position - followOffset, _followTransform.position, followSpeed) + followOffset;
            
            transform.position = target;
            transform.forward = -followOffset.normalized;
        }
    }
}
