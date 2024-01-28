using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class FireballController : NetworkBehaviour, ISpellLinearProjectile, ISpellTakesClientId
{
    [SerializeField] private float speed;
    [SerializeField] private float lifeTime;
    public GameObject player;
    [SerializeField] private float damage;
    [SerializeField] private GameObject explosion_prefab;
    [SerializeField] private Vector3 dir;
    public ulong playerID;

    void Awake(){
        Destroy(gameObject, lifeTime);
    }
    void OnTriggerEnter(Collider col){
        if (col.gameObject.CompareTag("Player") && col.GetComponent<NetworkBehaviour>().OwnerClientId != playerID){
            col.gameObject.GetComponent<PlayerCombatManager>().TakeDamage((int)damage);
            Destroy(gameObject);
        }
    }

    public override void OnDestroy(){
        if(explosion_prefab != null){
            GameObject explosion = Instantiate(explosion_prefab,transform.position,Quaternion.identity);
            explosion.GetComponent<FireballController>().player = player;
            explosion.GetComponent<FireballController>().playerID = playerID;
            explosion.GetComponent<NetworkObject>().Spawn();
        }
    }

    public void setDirection(Vector3 direction)
    {
        dir = direction;
        SetVelocityServerRpc();
    }

    [ServerRpc]
    void SetVelocityServerRpc(){
        GetComponent<Rigidbody>().velocity = dir * speed;
    }

    public void setPlayerId(ulong playerId)
    {
        playerID = playerId;
    }
}
