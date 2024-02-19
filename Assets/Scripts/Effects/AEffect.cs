using UnityEngine;

public enum ColliderType {
    BOX = 0,
    CAPSULE = 1,
    SPHERE = 2,
    MESH = 3
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