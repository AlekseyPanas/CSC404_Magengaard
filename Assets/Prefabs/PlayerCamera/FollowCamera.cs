using UnityEngine;


public class FollowCamera : MonoBehaviour 
{
    private Transform followTransform = null;

    public float followSpeed = 0.00001f;
    public Vector3 followDistance;

    void Start() {
        PlayerSpawnedEvent.OwnPlayerSpawnedEvent += setPlayer;  // Subscribe to receive info when the player spawns in
    }

    private void setPlayer(Transform player) {
        followTransform = player;
        followDistance = transform.position - followTransform.position;
    }

    void Update() {
        if (followTransform != null) {
            Vector3 target = Vector3.Lerp(transform.position - followDistance, followTransform.position, followSpeed) + followDistance;
            transform.position = target;
        }
    }
}
