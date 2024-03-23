using UnityEngine;

/** 
* Effect for when an object is being blown by wind
*/
public class WindEffect {
    public Vector3 SourcePosition { get; set; }
    public Vector3 Velocity { get; set; }  // Encodes wind direction and speed as relevant to target object
    public float ReflectDamageMultiplier {get; set;}
}
