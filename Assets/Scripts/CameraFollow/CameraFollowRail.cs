using System;
using UnityEngine;

/**
 * Animates the camera onto a rail, following the player.
 */
public class CameraFollowRail : ICameraFollow
{
    private readonly float _speed;
    private readonly CameraPosition _start;
    private readonly CameraPosition _end;
    
    private readonly float _adjustmentOffset;
    private readonly float _adjustmentMultiplier;

    /**
     * Initialize a follow with an origin position and forward, along with a rail vector that the
     *   camera can move along to reach the player.
     * Rail magnitude matters, if the player is far, the camera can move faster to reach it.
     *
     * Speed is a number between 0 and 1, the destination camera position is lerp'd with the
     *   current position using speed every frame.
     */
    public CameraFollowRail(CameraPosition start, CameraPosition end, float speed = 0.01f, float adjustmentOffset = 0.85f, float adjustmentMultiplier = 1.0f)
    {
        _speed = speed;
        _start = start;
        _end = end;
        _adjustmentOffset = adjustmentOffset;
        _adjustmentMultiplier = adjustmentMultiplier;
    }

    public CameraPosition FollowPosition(CameraFollowContext context)
    {
        if (context.Follow == null)
        {
            return _start;
        }
        
        // expected position = initial position + rail difference
        // rail difference = player proj onto rail - exp position proj onto rail

        var railDirection = _end.Position - _start.Position;
        var railDistance = railDirection.magnitude;
        var rail = railDirection / railDistance;

        if (railDistance <= 0)
        {
            return _start;
        } 
        
        var playerOnRail = Vector3.Project(context.Follow.Value - _start.Position, rail);

        var progress = Math.Clamp(playerOnRail.magnitude / railDistance * _adjustmentMultiplier - _adjustmentOffset, 0.0f, 1.0f);
        
        var expectedPosition = Vector3.Lerp(_start.Position, _end.Position, progress);
        var expectedForward = Vector3.Lerp(_start.Forward.normalized, _end.Forward.normalized, progress);
        
        return new CameraPosition
        {
            Position = Vector3.Lerp(context.Current.Position, expectedPosition, _speed),
            Forward = Vector3.Lerp(context.Current.Forward, expectedForward, _speed)
        };
    }
}