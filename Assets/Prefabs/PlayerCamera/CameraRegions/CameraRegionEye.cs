using ACameraControllable = AControllable<CameraManager, ControllerRegistrant>;
using UnityEngine;

/**
 * On player enter, switches the camera to the Fixed Follow.
 */
public class CameraRegionEye : ACameraRegion
{
    public Transform perspective;
    public float speed = 1.5f;
    public float sensitivity = 0.1f;

    protected override void OnTriggeredRegion(ACameraControllable manager, ControllerRegistrant controller, Collider player)
    {
        var follow = new CameraFollowEye(perspective.position, perspective.forward, speed, sensitivity);
        manager.GetSystem(controller).SwitchFollow(controller, follow);
    }
}