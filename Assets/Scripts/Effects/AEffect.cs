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
    public Collider col {get; private set;}
    public ColliderType colliderType {get; private set;}

    public AEffect SetCollider(Collider col, ColliderType colliderType) {
        this.col = col;
        this.colliderType = colliderType;
        return this;
    }
}