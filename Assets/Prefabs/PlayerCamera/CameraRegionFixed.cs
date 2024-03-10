using UnityEngine;

/**
 * On player enter, switches the camera to the Fixed Follow.
 */
public class CameraRegionFixed : MonoBehaviour
{
    public Transform perspective;
    public float speed = 1.5f;
    
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
        
        manager.SwitchFollow(this, new CameraFollowFixed(perspective.position, perspective.forward, speed));
    }
}
