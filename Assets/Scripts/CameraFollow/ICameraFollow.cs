using UnityEngine;

public struct CameraPosition
{
    public Vector3 Position;
    public Vector3 Forward;

    public static CameraPosition Zero = new()
    {
        Position = Vector3.zero,
        Forward = Vector3.forward
    };
}

public struct CameraFollowContext
{
    /**
     * Previous position of the camera given by the last ICameraFollow.
     *
     * CameraPosition.Zero if there was no previous ICameraFollow.
     */
    public CameraPosition Predecessor;
    /**
     * Current position of the camera.
     */
    public CameraPosition Current;
    /**
     * Position of the player follow.
     */
    public Vector3? Follow;
    /**
     * Time since this ICameraFollow has been in control of the camera.
     */
    public float TimeElapsed;
}

/**
 * Defines the behaviour of the camera movement in a region of the map.
 *
 * CameraManager invokes this.
 */
public interface ICameraFollow
{
    /**
     * Returns the camera position and direction given some contextual information.
     */
    CameraPosition FollowPosition(CameraFollowContext context);
}
