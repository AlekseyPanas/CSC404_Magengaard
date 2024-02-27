using UnityEngine;

/**
 * On player enter, the camera follows the player on a rail.
 */
public class CameraRegionRail : MonoBehaviour
{
    /**
     * Starting position of the camera. Set to some empty child.
     */
    public Transform start;

    /**
     * Ending position of the camera. Set to some empty child.
     */
    public Transform end;
    
    /**
     * Glide speed, every frame this the origin and destination camera are Lerp'd with this value.
     */
    public float speed = 0.01f;

    /**
     * Adjustment offset, use to put the camera slightly behind the player.
     * Value between 0 and 1.
     *
     * 0.15 means the camera will be 15% (in terms of distance between end - start) behind the player.
     */
    public float adjustmentOffset = 0.0f;
    
    /**
     * Adjustment multiplier, to slow progress towards the endpoint.
     * Value between 0 and 1.
     *
     * 0.85 means the player will need to travel 1 - 0.85 = 15% longer to reach the end.
     */
    public float adjustmentMultiplier = 1.0f;
    
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

        var startPosition = new CameraPosition { Position = start.position, Forward = start.forward };
        var endPosition = new CameraPosition { Position = end.position, Forward = end.forward };

        var follow = new CameraFollowRail(startPosition, endPosition, speed, adjustmentOffset, adjustmentMultiplier);
        
        manager.SwitchFollow(this, follow);
    }
}
