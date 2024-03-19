using UnityEngine;

public class CameraRegionSetFlag: MonoBehaviour
{
    public string flag;
    public bool add = true;
    
    public void OnTriggerEnter(Collider other)
    {
        var main = Camera.main;

        if (main == null)
        {
            return;
        }
        
        // We don't care about acquiring a lock on the camera manager.
        var manager = main.GetComponent<CameraManager>();

        if (add)
        {
            manager.Flags.Add(flag);
        }
        else
        {
            manager.Flags.Remove(flag);
        }
    }
}
