using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class VerticalBridge : NetworkBehaviour, IEffectListener<WindEffect> {

    private Rigidbody rigidbody;

    // Start is called before the first frame update
    void Awake() {
        rigidbody = GetComponent<Rigidbody>();
    }

    public void OnEffect(WindEffect effect) {
        rigidbody.isKinematic = false;
    }
}
