using UnityEngine;

public class CameraRegionRail : MonoBehaviour
{
    public Transform perspective;
    public Vector3 rail;
    public float speed = 0.01f;
    
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
        
        manager.SwitchFollow(this, new CameraFollowRail(perspective.position, perspective.forward, rail, speed));
    }
}