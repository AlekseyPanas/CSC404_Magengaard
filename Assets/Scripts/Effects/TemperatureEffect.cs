using UnityEngine;

/**
* Used for changing the temperature of an object. Used by fire and ice spells
*/
public class TemperatureEffect : IColliderEffect {
    public Collider Collider { get; set; }
    
    public float TempDelta { get; set; }
}
