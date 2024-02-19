using UnityEngine;

public enum ColliderType {
    NONE = 0,
    BOX = 1,
    CAPSULE = 2,
    SPHERE = 3,
    MESH = 4
}

/*
    Base class for effects. Passes collider information
*/
public abstract class AEffect {
    public Collider Collider {get; private set;}
    public ColliderType ColliderType {get; private set;}

    public AEffect SetCollider(Collider collider, ColliderType colliderType) {
        Collider = collider;
        ColliderType = colliderType;
        return this;
    }
}