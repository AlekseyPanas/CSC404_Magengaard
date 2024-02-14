using System;
using UnityEngine;

public class CameraFollowRail : ICameraFollow
{
    private readonly float _speed;
    private readonly Vector3 _position;
    private readonly Vector3 _forward;
    private readonly Vector3 _rail;
    private readonly float _magnitude;

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