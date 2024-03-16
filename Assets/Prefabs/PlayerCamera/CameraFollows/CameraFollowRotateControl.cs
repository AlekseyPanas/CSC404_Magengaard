using UnityEngine;

public class CameraFollowRotateControls : ICameraFollow
{
    private readonly float _offset;
    private readonly float _speed;
    private float _rot = 0;
    private DesktopControls _controls;
    
    private Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, Vector3 angles) {
        return Quaternion.Euler(angles) * (point - pivot) + pivot;
    }

    public CameraFollowRotateControls(float offset, float speed) {
        _offset = offset;
        _speed = speed;
        _controls = new DesktopControls();
        _controls.Enable();
        _controls.Game.Enable();
    }
    
    public CameraPosition FollowPosition(CameraFollowContext context) {
        if (context.Follow == null) { return context.Current; }

        _rot += 0.8f * _controls.Game.Movement.ReadValue<Vector2>().x;

        var expected = context.Follow - (new Vector3(1, 0, 0) * _offset); //+ (Vector3.up * 0.1f);
        expected = RotatePointAroundPivot((Vector3)expected, (Vector3)context.Follow, new Vector3(0, _rot, 0));
        expected += Vector3.up * 1f;

        return new CameraPosition {
            Position = (Vector3)expected ,//+ (Vector3.up * 0.2f),
            Forward = (Vector3)(context.Follow - expected)
        };
    }
}
