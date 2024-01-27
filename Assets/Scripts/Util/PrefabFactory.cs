using Unity.Netcode;
using Unity.Netcode.Components;
using Unity.VisualScripting;
using UnityEngine;


public class PrefabFactory {
    public static void SpawnLinearProjectileSpell(GameObject prefab, ulong playerId, Vector3 direction) {
        GameObject g = Object.Instantiate(prefab, 
            NetworkManager.Singleton.ConnectedClients[playerId].PlayerObject.transform.position, Quaternion.identity);
        g.GetComponent<ISpellLinearProjectile>().setDirection(direction);
        g.GetComponent<ISpellTakesClientId>().setPlayerId(playerId);
        g.GetComponent<NetworkObject>().Spawn();
    }

    public static void SpawnRemoteOriginSpell(GameObject prefab, ulong playerId, Vector3 origin) {
        GameObject g = Object.Instantiate(prefab, origin, Quaternion.identity);
        g.GetComponent<ISpellTakesClientId>().setPlayerId(playerId);
        g.GetComponent<NetworkObject>().Spawn();
    }
}
