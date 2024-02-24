using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class FireballProjectileController : NetworkBehaviour, ISpell
{
    [SerializeField] private float speed;
    [SerializeField] private float lifeTime;
    public GameObject player;
    [SerializeField] private GameObject explosion_prefab;
    private Vector3 dir;
    public ulong playerID;
    bool hasCollided = false;
    void Awake(){
        Invoke("DestroySpell", lifeTime);
    }
    void OnTriggerEnter(Collider col){
        if(!IsServer || hasCollided) return;
        if ((col.gameObject.CompareTag("Player") 
        && col.GetComponent<NetworkBehaviour>().OwnerClientId != playerID) 
        || col.gameObject.CompareTag("Enemy") 
        || col.gameObject.CompareTag("Ground")){
            hasCollided = true;
            DestroySpell(); 
        }
    }

    void SpawnExplosion(){
        if(!IsServer) return;
        GameObject explosion = Instantiate(explosion_prefab, transform.position + new Vector3(0,-0.45f,0),Quaternion.identity);
        explosion.GetComponent<FireballExplosionController>().player = player;
        explosion.GetComponent<FireballExplosionController>().playerID = playerID;
        explosion.GetComponent<NetworkObject>().Spawn();
    }
    void DestroySpell(){
        if(!IsOwner) return;
        SpawnExplosion();
        Destroy(gameObject);
    }

    public void setPlayerId(ulong playerId) { playerID = playerId; }

    public void preInitBackfire() { }

    public void preInit(SpellParamsContainer spellParams) {
        player = NetworkManager.Singleton.ConnectedClients[playerID].PlayerObject.gameObject;
        transform.position = player.transform.position;
        Direction3DSpellParams prms = new();
        prms.buildFromContainer(spellParams);
        dir = prms.Direction3D;
    }

    public void postInit() {
        transform.position += dir.normalized * Const.SPELL_SPAWN_DISTANCE_FROM_PLAYER + new Vector3(0,0.25f,0); //second number in the vector should be around the height of the player's waist
        GetComponent<Rigidbody>().velocity = dir * speed;
    }
}
