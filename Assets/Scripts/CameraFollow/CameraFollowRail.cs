using System;
using UnityEngine;

/**
 * Animates the camera onto a rail, following the player.
 */
public class CameraFollowRail : ICameraFollow
{
    private readonly float _speed;
    private readonly Vector3 _position;
    private readonly Vector3 _forward;
    private readonly Vector3 _rail;
    private readonly float _magnitude;

    /**
     * Initialize a follow with an origin position and forward, along with a rail vector that the
     *   camera can move along to reach the player.
     * Rail magnitude matters, if the player is far, the camera can move faster to reach it.
     *
     * Speed is a number between 0 and 1, the destination camera position is lerp'd with the
     *   current position using speed every frame.
     */
    public CameraFollowRail(Vector3 position, Vector3 forward, Vector3 rail, float speed = 0.01f)
    {
        _speed = speed;
        _position = position;
        _forward = forward;
        _rail = rail.normalized;
        _magnitude = rail.magnitude;
    }

    public CameraPosition FollowPosition(CameraFollowContext context)
    {
        if (context.Follow == null)
        {
            return new CameraPosition
            {
                Position = _position,
                Forward = _forward
            };
        }
        
        // expected position = initial position + rail difference
        // rail difference = player proj onto rail - exp position proj onto rail

        var playerOnRail = Vector3.Project(context.Follow.Value, _rail);
        var selfOnRail = Vector3.Project(context.Current.Position, _rail);

        var expectedPosition = _position + (playerOnRail - selfOnRail) * _magnitude;
        var expectedForward = _forward;
        
        return new CameraPosition
        {
            Position = Vector3.Lerp(context.Current.Position, expectedPosition, _speed),
            Forward = Vector3.Lerp(context.Current.Forward, expectedForward, _speed)
        };
    }
}