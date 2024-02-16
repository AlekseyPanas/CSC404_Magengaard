using UnityEngine;

/** 
* Effect for when an object is being blown by wind
*/
public class WindEffect {
    public Vector3 Velocity {get; private set;}  // Encodes wind direction and speed as relevant to target object

    public WindEffect setWindVelocity(Vector3 vel) {
        Velocity = vel;
        return this;
    }
}
