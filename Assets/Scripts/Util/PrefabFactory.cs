using Unity.Netcode;
using Unity.Netcode.Components;
using Unity.VisualScripting;
using UnityEngine;


public class PrefabFactory {
    public static void SpawnLinearProjectileSpell(GameObject prefab, ulong playerId, Vector3 direction) {
        var network = NetworkManager.Singleton.ConnectedClients[playerId].PlayerObject;
        var gameObject = network.transform;
        var ply = gameObject.transform.Find("PlayerBody").gameObject;
        
        var g = Object.Instantiate(prefab, 
            new Vector3(ply.transform.position.x, ply.GetComponent<CapsuleCollider>().bounds.min.y, ply.transform.position.z), Quaternion.identity);
        g.transform.GetComponent<NetworkObject>().Spawn();
        g.GetComponent<ISpellLinearProjectile>().setDirection(direction);
        g.GetComponent<ISpell>().setPlayerId(playerId);
        g.GetComponent<ISpell>().preInitSpell();
    }

    public static void SpawnRemoteOriginSpell(GameObject prefab, ulong playerId, Vector3 origin) {
        GameObject g = Object.Instantiate(prefab.gameObject, origin, Quaternion.identity);
        g.GetComponent<ISpell>().setPlayerId(playerId);
        g.GetComponent<ISpell>().preInitSpell();
        g.transform.GetComponent<NetworkObject>().Spawn();
    }
}
