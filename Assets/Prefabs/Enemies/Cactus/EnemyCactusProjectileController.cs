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
    void Start()
    {
        GetComponent<Rigidbody>().velocity = dir * speed;
        Destroy(gameObject, lifetime);
        transform.forward = dir.normalized;
    }

    void OnTriggerEnter(Collider col){
        if (col.CompareTag("Player")){
            IEffectListener<DamageEffect>.SendEffect(col.gameObject, new DamageEffect { Amount = (int)damage });
            Destroy(gameObject);
        }
        if (col.CompareTag("Ground")){
            Destroy(gameObject);
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
