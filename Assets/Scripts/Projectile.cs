using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Projectile: MonoBehaviour
{
    public float speed;
    public float lifeTime;
    public GameObject player;
    public float damage;

    void OnTriggerEnter(Collider col){
        if (col.gameObject.CompareTag("Player") && col.gameObject != player){
            col.gameObject.GetComponent<PlayerCombatManager>().TakeDamage((int)damage);
            Destroy(gameObject);
        }
    }
}
