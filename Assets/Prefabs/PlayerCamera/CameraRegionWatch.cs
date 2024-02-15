using UnityEngine;

/**
 * On player enter, the camera follows the player from some offset away.
 */
public class CameraRegionWatch : MonoBehaviour
{
    /**
     * Starting position of the camera. Set to some empty child.
     */
    public Vector3 offset;
    /**
     * Glide speed, every frame this the origin and destination camera are Lerp'd with this value.
     */
    public float speed = 0.1f;
    
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
        
        manager.SwitchFollow(this, new CameraFollowWatch(offset, speed));
    }
}
