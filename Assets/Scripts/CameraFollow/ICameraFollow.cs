using UnityEngine;

public struct CameraPosition
{
    public Vector3 Position;
    public Vector3 Forward;

    public static CameraPosition Zero = new CameraPosition
    {
        Position = Vector3.zero,
        Forward = Vector3.forward
    };
}

public struct CameraFollowContext
{
    public CameraPosition Predecessor;
    public CameraPosition Current;
    public Vector3? Follow;
    public float TimeElapsed;
}
    
public interface ICameraFollow
{
    CameraPosition FollowPosition(CameraFollowContext context);
}
