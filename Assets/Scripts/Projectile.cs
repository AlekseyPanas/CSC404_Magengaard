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
    [SerializeField] private GameObject explosion_prefab;

    void OnTriggerEnter(Collider col){
        if (col.gameObject.CompareTag("Player") && col.gameObject != player){
            col.gameObject.GetComponent<PlayerCombatManager>().TakeDamage((int)damage);
            Destroy(gameObject);
        }
    }

    void OnDestroy(){
        if(explosion_prefab != null){
            GameObject explosion = Instantiate(explosion_prefab,transform.position,Quaternion.identity);
            explosion.GetComponent<Projectile>().player = player;
            Destroy(explosion, explosion.GetComponent<Projectile>().lifeTime);
        }
    }
}
