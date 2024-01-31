using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class SandstormScript : NetworkBehaviour, ISpellLinearProjectile, ISpell {

    private static readonly float TIME_TO_LIVE = 10;

    private Vector3 direction;
    private ulong playerId;
    float start_time;

    // Start is called before the first frame update
    void Start() {}

    // Update is called once per frame
    void Update() {
        if (IsServer) {
            transform.position += direction.normalized * 0.01f; // Move Sandstorm

            if (Time.time - start_time > TIME_TO_LIVE) { // Kill after some time, TODO: Replace with collision kill
                Destroy(this.gameObject);
            }
        }
    }

    void OnTriggerEnter(Collider other) {
        if (IsServer) {
            if (!other.gameObject.CompareTag("Player") && !other.gameObject.CompareTag("SpellSandstorm")) {
                Destroy(gameObject);
            } else if (other.gameObject.CompareTag("Player") && playerId != other.GetComponent<NetworkBehaviour>().OwnerClientId) {
                // INSERT DAMAGE EVENT ON OPPONENT WITH CLIENT ID = playerId
            }
        }
    }

    public void setDirection(Vector3 direction) {
        this.direction = direction;
    }

    public void setPlayerId(ulong playerId) {
        this.playerId = playerId;
    }

    public void preInitSpell() {
        // Shift position so that the sandstorm is not spawned directly on top of the player
        transform.position += direction.normalized * Const.SPELL_SPAWN_DISTANCE_FROM_PLAYER;

        // Sandstorm doesn't move vertically
        direction.y = 0;

        // Captures start time
        start_time = Time.time;

        // Shift up so bounding box is exactly above the ground
        transform.position = new Vector3(transform.position.x, 
                                        transform.position.y + (transform.position.y - gameObject.GetComponent<CapsuleCollider>().bounds.min.y) + 0.01f, 
                                        transform.position.z);
    }
}
