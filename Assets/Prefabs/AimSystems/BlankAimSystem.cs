using UnityEngine;

public class BlankAimSystem : AAimSystem
{

    public override void setPlayerTransform(Transform playerTransform) {}

    public override void toggleInput(bool doAcceptInput) {}

    // Fire event with blank params and destroy instantly
    void Start() {
        invokeAimingFinishedEvent(null);
        Destroy(gameObject);
    }
}
