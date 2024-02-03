using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System;
using Unity.Mathematics;

public class ElectroSphereController : NetworkBehaviour, ISpell {
    [SerializeField] private float speed;
    [SerializeField] private float lifeTime;
    [SerializeField] private GameObject target;
    [SerializeField] private float damage;
    [SerializeField] private ulong playerID;
    [SerializeField] private float damageTick;
    [SerializeField] private float damageTimer;
    Vector3 dir;

    void Awake(){
        Destroy(gameObject, lifeTime);
    }

    void Update(){
        if(target == null){
            return;
        }
        Vector3 diff = target.transform.position - transform.position;
        dir = new Vector3(diff.x, 0, diff.z).normalized;
        SetVelocityServerRpc();
    }
    void OnTriggerEnter(Collider col){
        if (col.gameObject.CompareTag("Player") && col.GetComponent<NetworkBehaviour>().OwnerClientId != playerID){
            if(Time.time > damageTimer){
                col.gameObject.GetComponent<Entity>().Damage(new DamageParameters
                {
                    damage = 5
                });
                damageTimer = Time.time + damageTick;
            }
        }
    }
    [ServerRpc]
    void SetVelocityServerRpc(){
        GetComponent<Rigidbody>().velocity = dir * speed;
    }

    public void setPlayerId(ulong playerId)
    {
        playerID = playerId;
    }

    public void setParams(Direction2DSpellParams spellParams)
    {
        throw new NotImplementedException();
    }

    public void setParams()
    {
        throw new NotImplementedException();
    }

    public void preInitSpell()
    {
        if(!IsOwner) return;
        Destroy(gameObject, lifeTime);
        damageTimer = Time.time;
        List<GameObject> players = new List<GameObject>(GameObject.FindGameObjectsWithTag("Player"));
        List<GameObject> otherPlayers = new List<GameObject>();
        foreach (GameObject p in players){
            if (p.GetComponent<NetworkBehaviour>().OwnerClientId != playerID) {
                otherPlayers.Add(p);
            }
        }
        float minDistance = math.INFINITY;
        foreach (GameObject p in otherPlayers){
            float distance = (p.transform.position - transform.position).magnitude;
            if (distance < minDistance){
                minDistance = distance;
                target = p;
            }
        }
        dir = Vector3.zero;
        transform.position += new Vector3(0, transform.localScale.y / 2, 0);
    }
}
