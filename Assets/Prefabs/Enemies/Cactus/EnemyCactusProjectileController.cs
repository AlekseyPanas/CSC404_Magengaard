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
    List<GameObject> collided;
    void Start()
    {
        GetComponent<Rigidbody>().velocity = dir * speed;
        Destroy(gameObject, lifetime);
        transform.forward = dir.normalized;
        collided = new List<GameObject>();
    }

    void OnTriggerEnter(Collider col){
        if(!collided.Contains(col.gameObject)){
            IEffectListener<DamageEffect>.SendEffect(col.gameObject, new DamageEffect { Amount = (int)damage, SourcePosition = transform.position });
            if(col.CompareTag("Ground") || col.CompareTag("Player")){
                Destroy(gameObject);
            }
            collided.Add(col.gameObject);
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
    }
}
