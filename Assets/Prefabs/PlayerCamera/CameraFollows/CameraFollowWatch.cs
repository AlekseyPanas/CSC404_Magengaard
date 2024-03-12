using UnityEngine;

public class CameraFollowWatch : ICameraFollow
{
    private readonly Vector3 _offset;
    private readonly float _speed;
    
    public CameraFollowWatch(Vector3 offset, float speed)
    {
        _offset = offset;
        _speed = speed;
    }
    
    public CameraPosition FollowPosition(CameraFollowContext context)
    {
        if (context.Follow == null)
        {
            return context.Current;
        }

        var expected = context.Follow.Value + _offset;

        return new CameraPosition
        {
            Position = Vector3.Lerp(context.Current.Position, expected, _speed),
            Forward = (-_offset).normalized
        };
    }
}
