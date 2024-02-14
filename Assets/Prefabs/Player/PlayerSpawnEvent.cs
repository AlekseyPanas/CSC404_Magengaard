using Unity.Netcode;
using UnityEngine;

/**
* Generates an event on startup for the player owned by this client. Listeners of the delegate
* can set a reference to the spawned client player (e.g the main camera)
*/
public class PlayerSpawnedEvent: NetworkBehaviour {
    public delegate void OwnPlayerSpawned(Transform playerTransform);
    public static event OwnPlayerSpawned OwnPlayerSpawnedEvent = _ => { };

    void Start() {
        // If we don't have a network manager present 
        if (IsOwner)
        {
            // Hijacking a Player Spawn Script
            // For setting the respawn location.
            var spawn = GameObject.FindWithTag("Respawn");

            var local = transform;

            if (spawn != null)
            {
                local.position = spawn.transform.position;
                local.forward = spawn.transform.forward;
            }

            OwnPlayerSpawnedEvent(local);
        }
    }
}
