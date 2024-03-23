using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class EnemyCactusProjectileController : NetworkBehaviour, IEffectListener<WindEffect>
{
    Vector3 dir;
    [SerializeField] private float speed;
    [SerializeField] private float lifetime;
    [SerializeField] private float damage;
    [SerializeField] private GameObject sender;
    [SerializeField] private Rigidbody rb;
    List<GameObject> collided;
    bool isReflected = false;
    void Start()
    {
        rb.velocity = dir * speed;
        Destroy(gameObject, lifetime);
        transform.forward = dir.normalized;
        collided = new List<GameObject>();
    }

    void OnTriggerEnter(Collider col){
        if(!collided.Contains(col.gameObject)){
            if((!isReflected && col.CompareTag("Player")) || (isReflected && col.CompareTag("Enemy"))){
                IEffectListener<DamageEffect>.SendEffect(col.gameObject, new DamageEffect { Amount = (int)damage, SourcePosition = transform.position });
            }
            if(col.CompareTag("Ground") || col.CompareTag("Player") || (isReflected && col.CompareTag("Enemy"))){
                Destroy(gameObject);
            }
            collided.Add(col.gameObject);
        }
    }

    void Update(){
        if(isReflected){
            dir = (sender.transform.position - transform.position).normalized;
            rb.velocity = dir * speed;
            transform.forward = dir;
        }
    }

    public void SetTargetDirection(Vector3 d){
        dir = d;
    }

    public void OnEffect(WindEffect effect)
    {
        dir = effect.Velocity.normalized;
        GetComponent<Rigidbody>().velocity = dir * speed;
        transform.forward = dir.normalized;
        isReflected = true;
        damage *= effect.ReflectDamageMultiplier;
    }

    public void SetSender(GameObject g){
        sender = g;
    }
}
