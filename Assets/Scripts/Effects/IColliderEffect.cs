using UnityEngine;


/*
    Base class for effects. Passes collider information
*/
public interface IColliderEffect {
    public Collider Collider { get; set; }
}
