using UnityEngine;

public class Direction2DAimSystem : AAimSystem
{
    private Transform ownPlayerTransform;

    public override void setPlayerTransform(Transform playerTransform) {ownPlayerTransform = playerTransform;}

    public override void toggleInput(bool doAcceptInput)
    {
        throw new System.NotImplementedException();
    }

    // Start is called before the first frame update
    void Start() {

        // TODO: This code temporarily gives back a gibberish vector and self destroys
        invokeAimingFinishedEvent(new SpellParamsContainer().setVector3(0, new Vector3(2,1,0)));
        Destroy(gameObject);
    }

    // Update is called once per frame
    void Update() {
        
    }
}
