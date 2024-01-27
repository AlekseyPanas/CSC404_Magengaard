using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System;
using Unity.Mathematics;

public class ElectroSphereController : MonoBehaviour, ISpellTakesClientId {
    [SerializeField] private float speed;
    [SerializeField] private float lifeTime;
    [SerializeField] private GameObject target;
    [SerializeField] private float damage;
    [SerializeField] private ulong playerID;
    [SerializeField] private float damageTick;
    [SerializeField] private float damageTimer;

    void Awake(){
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
    }

    void Update(){
        if(target == null){
            return;
        }
        Vector3 diff = target.transform.position - transform.position;
        Vector3 diffXZ = new Vector3(diff.x, 0, diff.z).normalized;
        GetComponent<Rigidbody>().velocity = diffXZ * speed;
    }
    void OnTriggerEnter(Collider col){
        if (col.gameObject.CompareTag("Player") && col.GetComponent<NetworkBehaviour>().OwnerClientId != playerID){
            if(Time.time > damageTimer){
                col.gameObject.GetComponent<PlayerCombatManager>().TakeDamage((int)damage);
                damageTimer = Time.time + damageTick;
            }
        }
    }

    public void setPlayerId(ulong playerId)
    {
        playerID = playerId;
    }
}
