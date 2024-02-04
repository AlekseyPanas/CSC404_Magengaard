using UnityEngine;

public class BlankAimSystem : AAimSystem
{

    public override void setPlayerTransform(Transform playerTransform) {}

    // Fire event with blank params and destroy instantly
    void Start() {
        invokeAimingFinishedEvent(new BlankSpellParams());
        Destroy(gameObject);
    }
}
