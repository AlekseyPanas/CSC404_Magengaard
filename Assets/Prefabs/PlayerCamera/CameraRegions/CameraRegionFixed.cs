using ACameraControllable = AControllable<CameraManager, ControllerRegistrant>;
using UnityEngine;

/**
 * On player enter, switches the camera to the Fixed Follow.
 */
public class CameraRegionFixed : ACameraRegion
{
    public Transform perspective;
    public float speed = 1.5f;

    public string requiredFlag;

    protected override void OnTriggeredRegion(ACameraControllable manager, ControllerRegistrant controller, Collider player)
    {
        var cameraManager = manager.GetSystem(controller);

        if (!string.IsNullOrEmpty(requiredFlag) && !cameraManager.Flags.Contains(requiredFlag))
        {
            return;
        }
        
        var follow = new CameraFollowFixed(perspective.position, perspective.forward, speed);
        cameraManager.SwitchFollow(controller, follow);
    }
}
