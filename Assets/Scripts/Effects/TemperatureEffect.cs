using UnityEngine;

/**
* Used for changing the temperature of an object. Used by fire and ice spells
*/
public class TemperatureEffect : IMeshEffect {
    public GameObject mesh { get; set; }
    
    public float TempDelta { get; set; }
}
