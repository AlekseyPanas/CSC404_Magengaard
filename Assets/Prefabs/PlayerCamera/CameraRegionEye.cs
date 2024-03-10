using UnityEngine;

/**
 * On player enter, switches the camera to the Fixed Follow.
 */
public class CameraRegionEye : MonoBehaviour
{
    public Transform perspective;
    public float speed = 1.5f;
    public float sensitivity = 0.1f;
    
    private void OnTriggerEnter(Collider other)
    {
        var current = Camera.main;

        if (current == null)
        {
            return;
        }
        
        var manager = current.GetComponent<CameraManager>();

        if (manager == null)
        {
            return;
        }

        var follow = new CameraFollowEye(perspective.position, perspective.forward, speed, sensitivity);
        
        manager.SwitchFollow(this, follow);
    }
}