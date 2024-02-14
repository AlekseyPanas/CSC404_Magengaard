using System;
using UnityEngine;

public class CameraFollowFixed : ICameraFollow
{
    private readonly float _speed;
    private readonly Vector3 _position;
    private readonly Vector3 _forward;

    private static float Ease(float value)
    {
        // Sin Easing
        // return (float)Math.Sin(value * Math.PI / 2);
        
        // Cubic Easing
        var back = 1 - value;
        return 1 - back * back * back;
    }
    
    public CameraFollowFixed(Vector3 position, Vector3 forward, float speed = 1.5f)
    {
        _speed = speed;
        _position = position;
        _forward = forward;
    }

    public CameraPosition FollowPosition(CameraFollowContext context)
    {
        if (_speed == 0.0)
        {
            return new CameraPosition
            {
                Position = _position,
                Forward = _forward,
            };
        }
        
        var progress = Math.Min(context.TimeElapsed / _speed, 1.0f);
        var value = Ease(progress);

        return new CameraPosition
        {
            Position = Vector3.Lerp(context.Predecessor.Position, _position, value),
            Forward = Vector3.Lerp(context.Predecessor.Forward, _forward, value),
        };
    }
}
