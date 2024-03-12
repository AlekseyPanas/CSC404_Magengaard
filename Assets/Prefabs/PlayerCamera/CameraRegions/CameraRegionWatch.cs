using ACameraControllable = AControllable<CameraManager, ControllerRegistrant>;
using UnityEngine;

/**
 * On player enter, the camera follows the player from some offset away.
 */
public class CameraRegionWatch : ACameraRegion
{
    /**
     * Starting position of the camera. Set to some empty child.
     */
    public Vector3 offset;
    /**
     * Glide speed, every frame this the origin and destination camera are Lerp'd with this value.
     */
    public float speed = 0.1f;

    protected override void OnTriggeredRegion(ACameraControllable manager, ControllerRegistrant controller, Collider player)
    {
        var follow = new CameraFollowWatch(offset, speed);
        manager.GetSystem(controller).SwitchFollow(controller, follow);
    }
}
