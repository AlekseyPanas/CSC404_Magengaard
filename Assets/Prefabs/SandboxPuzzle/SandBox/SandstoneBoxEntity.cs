using Unity.Netcode;
using UnityEngine;
using FMODUnity;

public class SandstoneBoxEntity : NetworkBehaviour, IEffectListener<WindEffect>
{
    private Rigidbody rigidBody;
    private StudioEventEmitter audioSys;

    void Awake() {
        audioSys = GetComponent<StudioEventEmitter>();
    }

    void IEffectListener<WindEffect>.OnEffect(WindEffect effect) {
        if (IsServer) {
            rigidBody.AddForce(effect.Velocity.normalized * 3, ForceMode.Impulse);
            audioSys.Play();
        }
    }

    void freezeY() {
        rigidBody.constraints |= RigidbodyConstraints.FreezePositionY;
    }

    // Start is called before the first frame update
    void Start() {
        rigidBody = GetComponent<Rigidbody>();
        Invoke("freezeY", 0.5f);
    }

    // Update is called once per frame
    void Update() { }

    void OnTriggerEnter(Collider trigger) {
        Debug.Log("I COLLIDED BVUDD");
    }
}
