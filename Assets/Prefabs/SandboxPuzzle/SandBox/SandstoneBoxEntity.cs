using System.Collections;
using System.Collections.Generic;
using IngameDebugConsole;
using Unity.Netcode;
using UnityEngine;

public class SandstoneBoxEntity : NetworkBehaviour, IEffectListener<WindEffect>
{
    private Rigidbody rigidBody;

    void IEffectListener<WindEffect>.OnEffect(WindEffect effect) {
        if (IsServer) {
            rigidBody.AddForce(effect.Velocity.normalized * 3, ForceMode.Impulse);
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
    void Update() {
        
    }

    
}
