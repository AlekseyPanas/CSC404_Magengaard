using ACameraControllable = AControllable<CameraManager, ControllerRegistrant>;
using UnityEngine;

/**
 * On player enter, switches the camera to the Fixed Follow.
 */
public class CameraRegionFixed : ACameraRegion
{
    public Transform perspective;
    public float speed = 1.5f;

    protected override void OnTriggeredRegion(ACameraControllable manager, ControllerRegistrant controller, Collider player)
    {
        var follow = new CameraFollowFixed(perspective.position, perspective.forward, speed);
        manager.GetSystem(controller).SwitchFollow(controller, follow);
    }
}
