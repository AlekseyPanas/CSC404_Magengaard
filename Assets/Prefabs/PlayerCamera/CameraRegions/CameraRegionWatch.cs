using ACameraControllable = AControllable<CameraManager, ControllerRegistrant>;
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
    
    private void OnTriggerEnter(Collider other) {
        // Try get camera
        var current = Camera.main;
        if (current == null) { return; }
        
        // Try get camera controllable/manager
        var manager = current.GetComponent<ACameraControllable>();
        if (manager == null) { return; }

        // Try to register as a controller
        var controller = manager.RegisterController((int)CameraControllablePriorities.REGION);
        if (controller == null) { return; }

        // If all is well, inject corresponding follow
        var follow = new CameraFollowWatch(offset, speed);
        manager.GetSystem(controller).SwitchFollow(controller, follow);
    }
}
