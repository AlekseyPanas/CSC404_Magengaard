using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyCactusProjectileController : MonoBehaviour
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

    void Update()
    {
        
    }

    void OnTriggerEnter(Collider col){
        if (col.CompareTag("Player")){
            IEffectListener<DamageEffect>.SendEffect(col.gameObject, new DamageEffect().SetDamageAmount((int)damage));
            Destroy(gameObject);
        }
    }

    public void SetTargetDirection(Vector3 d){
        dir = d;
    }
}
