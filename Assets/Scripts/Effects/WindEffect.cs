using UnityEngine;

/** 
* Effect for when an object is being blown by wind
*/
public class WindEffect : Effect{
    public Vector3 Velocity { get; set; }  // Encodes wind direction and speed as relevant to target object
    public float KBMultiplier { get; set; }
    public float ReflectDamageMultiplier { get; set; }
    public GameObject DeflectionParticle { get; set; }
}
