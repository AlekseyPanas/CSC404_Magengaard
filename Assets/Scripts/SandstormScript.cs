using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class SandstormScript : NetworkBehaviour, ISpellLinearProjectile, ISpellTakesClientId {


    private Vector3 direction;

    // Start is called before the first frame update
    void Start() {
        
    }

    // Update is called once per frame
    void Update() {
        
    }

    public void setDirection(Vector3 direction) {
        if (this.direction == null) {
            this.direction = direction;
        }
    }

    public void setPlayerId(ulong playerId) {
        
    }
}
