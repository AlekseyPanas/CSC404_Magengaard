using ACameraControllable = AControllable<CameraManager, ControllerRegistrant>;
using UnityEngine;

/**
 * On player enter, switches the camera to the Fixed Follow.
 */
public class CameraRegionEye : MonoBehaviour
{
    public Transform perspective;
    public float speed = 1.5f;
    public float sensitivity = 0.1f;
    
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
        var follow = new CameraFollowEye(perspective.position, perspective.forward, speed, sensitivity);
        manager.GetSystem(controller).SwitchFollow(controller, follow);
    }
}