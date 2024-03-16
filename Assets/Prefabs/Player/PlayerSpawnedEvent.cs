using System.Linq;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

/**
* Generates an event on startup for the player owned by this client. Listeners of the delegate
* can set a reference to the spawned client player (e.g the main camera)
*/
public class PlayerSpawnedEvent: NetworkBehaviour {
    public delegate void OwnPlayerSpawned(Transform playerTransform);
    public static event OwnPlayerSpawned OwnPlayerSpawnedEvent = _ => { };

    override public void OnNetworkSpawn() {
        // If we don't have a network manager present 
        if (IsOwner)
        {
            // Hijacking a Player Spawn Script
            // For setting the respawn location.
            GameObject[] spawns = GameObject.FindGameObjectsWithTag("Respawn");
            GameObject spawn = spawns.ToList().FindLast(s => s.GetComponent<RespawnPoint>().isLevelSpawn);

            var local = transform;

            if (spawn != null)
            {
                GetComponent<CharacterController>().enabled = false;
                local.position = spawn.transform.position;
                local.forward = spawn.transform.forward;
                GetComponent<CharacterController>().enabled = true;
            }

            OwnPlayerSpawnedEvent(local);
        }
    }

    void Update () {}
}
