using Unity.Netcode;
using UnityEngine;

/**
* Generates an event on startup for the player owned by this client. Listeners of the delegate
* can set a reference to the spawned client player (e.g the main camera)
*/
public class PlayerCameraLinkEvent: NetworkBehaviour {
    public delegate void OwnPlayerSpawned(Transform playerTransform);
    public static event OwnPlayerSpawned OwnPlayerSpawnedEvent;

    void Start() {
        if (IsOwner) {
            OwnPlayerSpawnedEvent(transform);
        }
    }
}
