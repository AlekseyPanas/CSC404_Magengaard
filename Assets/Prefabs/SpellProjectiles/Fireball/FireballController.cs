using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class FireballController : NetworkBehaviour, ISpell
{
    [SerializeField] private float speed;
    [SerializeField] private float lifeTime;
    public GameObject player;
    [SerializeField] private float damage;
    [SerializeField] private GameObject explosion_prefab;
    private Vector3 dir;
    [SerializeField] private bool isExplosion;
    public ulong playerID;

    void Awake(){
        Invoke("DestroySpell", lifeTime);
    }
    void OnTriggerEnter(Collider col){
        if(IsServer){
            if(col.gameObject.CompareTag("Ground")) {
                if(!isExplosion) DestroySpell();
            }
            else if ((col.gameObject.CompareTag("Player") && 
                    col.GetComponent<NetworkBehaviour>().OwnerClientId != playerID) || 
                    col.gameObject.CompareTag("Enemy")){
                if (isExplosion) {
                    IEffectListener<DamageEffect>.SendEffect(col.gameObject, new DamageEffect().SetDamageAmount((int)damage));
                }
                else { DestroySpell(); }
            }
        }
    }
    void DestroySpell(){
        if(!IsOwner) return;
        if(explosion_prefab != null){
            GameObject explosion = Instantiate(explosion_prefab, transform.position + new Vector3(0,-0.45f,0),Quaternion.identity);
            explosion.GetComponent<FireballController>().player = player;
            explosion.GetComponent<FireballController>().playerID = playerID;
            explosion.GetComponent<NetworkObject>().Spawn();
        }
        Destroy(gameObject);
    }

    public void setPlayerId(ulong playerId) { playerID = playerId; }

    public void preInitBackfire() { }

    public void preInit(SpellParamsContainer spellParams) {
        //Debug.Log("Are we on the server? " + IsServer);  // NO
        player = NetworkManager.Singleton.ConnectedClients[playerID].PlayerObject.gameObject;
        transform.position = player.transform.position;
        
        Direction3DSpellParams prms = new();
        prms.buildFromContainer(spellParams);
        dir = prms.Direction3D;
    }

    public void postInit() {
        //Debug.Log("Are we on the server NOW? " + IsServer);  // YES
        transform.position += dir.normalized * Const.SPELL_SPAWN_DISTANCE_FROM_PLAYER + new Vector3(0,0.25f,0); //second number in the vector should be around the height of the player's waist
        GetComponent<Rigidbody>().velocity = dir * speed;
    }
}
