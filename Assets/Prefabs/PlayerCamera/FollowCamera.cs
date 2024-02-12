using UnityEngine;


public class FollowCamera : MonoBehaviour 
{
    private Transform followTransform = null;

    public float followSpeed = 0.00001f;
    public Vector3 followDistance;
    public Vector3 followOffset;
    public float followZoom;

    void Start() {
        PlayerSpawnedEvent.OwnPlayerSpawnedEvent += setPlayer;  // Subscribe to receive info when the player spawns in
    }

    private void setPlayer(Transform player) {
        followTransform = player;
        followDistance = followTransform.position + followOffset * followZoom;
    }

    void Update() {
        if (followTransform != null) {
            Vector3 target = Vector3.Lerp(transform.position - followDistance, followTransform.position, followSpeed) + followDistance;
            transform.position = target;
        }
    }
}
