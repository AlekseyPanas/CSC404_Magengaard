using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class FireballController : NetworkBehaviour, ISpellLinearProjectile, ISpell
{
    [SerializeField] private float speed;
    [SerializeField] private float lifeTime;
    public GameObject player;
    [SerializeField] private float damage;
    [SerializeField] private GameObject explosion_prefab;
    [SerializeField] private Vector3 dir;
    [SerializeField] private bool isExplosion;
    public ulong playerID;

    void Awake(){
        Invoke("DestroySpell", lifeTime);
    }
    void OnTriggerEnter(Collider col){
        if(IsServer){
            if(col.gameObject.CompareTag("Ground")){
                if(!isExplosion) DestroySpell();
            }
            else if (col.gameObject.CompareTag("Player") && col.GetComponent<NetworkBehaviour>().OwnerClientId != playerID){
                col.gameObject.GetComponent<PlayerCombatManager>().TakeDamage((int)damage);
                if(!isExplosion) DestroySpell();
            }
        }
    }
    void DestroySpell(){
        if(!IsOwner) return;
        if(explosion_prefab != null){
            FireExplosionServerRpc();
        }
        Destroy(gameObject);
    }

    [ServerRpc]
    void FireExplosionServerRpc(){
        GameObject explosion = Instantiate(explosion_prefab,transform.position + new Vector3(0,-0.45f,0),Quaternion.identity);
        explosion.GetComponent<FireballController>().player = player;
        explosion.GetComponent<FireballController>().playerID = playerID;
        explosion.GetComponent<NetworkObject>().Spawn();
    }

    public void setDirection(Vector3 direction)
    {
        dir = direction;
    }

    public void setPlayerId(ulong playerId)
    {
        playerID = playerId;
    }

    public void preInitSpell()
    {
        transform.position += dir.normalized * Const.SPELL_SPAWN_DISTANCE_FROM_PLAYER + new Vector3(0,0.5f,0);
        GetComponent<Rigidbody>().velocity = dir * speed;
    }
}
