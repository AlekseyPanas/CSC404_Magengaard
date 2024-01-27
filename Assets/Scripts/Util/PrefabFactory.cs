using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;


public class PrefabFactory {
    public static void SpawnLinearProjectileSpell(GameObject prefab, ulong playerId, Vector3 direction) {
        GameObject ply = NetworkManager.Singleton.ConnectedClients[playerId].PlayerObject.
            gameObject.transform.Find("Player").gameObject;

        GameObject g = Object.Instantiate(prefab, 
            new Vector3(ply.transform.position.x, ply.GetComponent<CapsuleCollider>().bounds.min.y, ply.transform.position.z), Quaternion.identity);
        g.GetComponent<ISpellLinearProjectile>().setDirection(direction);
        g.GetComponent<ISpell>().setPlayerId(playerId);
        g.GetComponent<ISpell>().preInitSpell();
        g.transform.GetComponent<NetworkObject>().Spawn();
    }

    public static void SpawnRemoteOriginSpell(GameObject prefab, ulong playerId, Vector3 origin) {
        GameObject g = Object.Instantiate(prefab.gameObject, origin, Quaternion.identity);
        g.GetComponent<ISpell>().setPlayerId(playerId);
        g.GetComponent<ISpell>().preInitSpell();
        g.transform.GetComponent<NetworkObject>().Spawn();
    }
}
