using UnityEngine;

public class Direction2DAimSystem : AAimSystem
{
    private Transform ownPlayerTransform;

    public override void setPlayerTransform(Transform playerTransform) {ownPlayerTransform = playerTransform;}

    // Start is called before the first frame update
    void Start() {

        // TODO: This code temporarily gives back a gibberish vector and self destroys
        invokeAimingFinishedEvent(new Direction2DSpellParams(new Vector2(2,1)));
        Destroy(gameObject);
    }

    // Update is called once per frame
    void Update() {
        
    }
}
