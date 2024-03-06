using System;
using UnityEngine;

/**
 * Animates the camera to a position and then *slightly* follows the player from that position.
 */
public class CameraFollowEye: ICameraFollow
{
    private readonly float _time;
    private readonly Vector3 _position;
    private readonly Vector3 _forward;

    private readonly float _sensitivity;

    private static float Ease(float value)
    {
        // Sin Easing
        // return (float)Math.Sin(value * Math.PI / 2);
    
        // Cubic Easing
        var back = 1 - value;
        return 1 - back * back * back;
    }

    /**
     * Initialized a follow with a target position/forward and a time for the animation to take.
     */
    public CameraFollowEye(Vector3 position, Vector3 forward, float time = 1.5f, float sensitivity = 0.1f)
    {
        _time = time;
        _position = position;
        _forward = forward;
        _sensitivity = sensitivity;
    }

    private Vector3 ModifyForward(Vector3 position, Vector3 forward, float easing, Vector3? follow)
    {
        if (!follow.HasValue)
        {
            return forward;
        }

        var look = follow.Value - position;

        return Vector3.Lerp(forward.normalized, look.normalized, easing * _sensitivity).normalized;
    }
    
    public CameraPosition FollowPosition(CameraFollowContext context)
    {
        if (_time <= 0.0)
        {
            return new CameraPosition
            {
                Position = _position,
                Forward = ModifyForward(context.Current.Position, _forward, 1.0f, context.Follow),
            };
        }
    
        var progress = Math.Min(context.TimeElapsed / _time, 1.0f);
        var value = Ease(progress);

        var forward = Vector3.Lerp(context.Predecessor.Forward, _forward, value);
        
        return new CameraPosition
        {
            Position = Vector3.Lerp(context.Predecessor.Position, _position, value),
            Forward = ModifyForward(context.Current.Position, forward, value, context.Follow),
        };
    }
}
    