using UnityEngine;

/**
 * Positions a camera on a rail that follows the player.
 */
public class CameraRegionRail : MonoBehaviour
{
    /**
     * Starting position of the camera. Set to some empty child.
     */
    public Transform perspective;
    /**
     * Rail vector/direction of the camera. This camera will only move in this direction.
     */
    public Vector3 rail;
    /**
     * Glide speed, every frame this the origin and destination camera are Lerp'd with this value.
     */
    public float speed = 0.01f;
    
    private void OnTriggerEnter(Collider other)
    {
        var current = Camera.main;

        if (current == null)
        {
            return;
        }
        
        var manager = current.GetComponent<CameraManager>();

        if (manager == null)
        {
            return;
        }
        
        manager.SwitchFollow(this, new CameraFollowRail(perspective.position, perspective.forward, rail, speed));
    }
}