using ACameraControllable = AControllable<CameraManager, ControllerRegistrant>;
using UnityEngine;

/**
 * On player enter, the camera follows the player from some offset away.
 */
public class CameraRegionRotateControls : ACameraRegion
{
    /**
     * Starting position of the camera. Set to some empty child.
     */
    public float offset;
    /**
     * Glide speed, every frame this the origin and destination camera are Lerp'd with this value.
     */
    public float speed = 0.1f;
    
    protected override void OnTriggeredRegion(ACameraControllable manager, ControllerRegistrant controller, Collider player) {
        manager.GetSystem(controller).SwitchFollow(controller, new CameraFollowRotateControls(offset, speed));
    }
}
