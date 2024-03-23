using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class EnemyCactusProjectileController : NetworkBehaviour, IEffectListener<WindEffect>
{
    Vector3 dir;
    [SerializeField] private float speed;
    [SerializeField] private float lifetime;
    [SerializeField] private float rotationSpeed;
    [SerializeField] private float damage;
    [SerializeField] private float homingThreshold;
    [SerializeField] private GameObject sender;
    [SerializeField] Transform model;
    [SerializeField] private Rigidbody rb;
    
    List<GameObject> collided;
    bool isReflected = false;
    bool isHoming = false;
    void Start()
    {
        rb.velocity = dir * speed;
        Destroy(gameObject, lifetime);
        transform.forward = dir.normalized;
        collided = new List<GameObject>();
    }

    void OnTriggerEnter(Collider col){
        if(!isReflected && col.gameObject == sender) return;
        if(!collided.Contains(col.gameObject)){
            if((!isReflected && col.CompareTag("Player")) || (isReflected && col.CompareTag("Enemy"))){
                IEffectListener<DamageEffect>.SendEffect(col.gameObject, new DamageEffect { Amount = (int)damage, SourcePosition = transform.position });
            }
            if(col.CompareTag("Ground") || (!isReflected && col.CompareTag("Player")) || (isReflected && col.CompareTag("Enemy"))){
                Destroy(gameObject);
            }
            collided.Add(col.gameObject);
        }
    }

    void Update(){
        model.Rotate(0, 0, rotationSpeed);
        if(isReflected && isHoming){
            DeflectHoming();
        }
    }

    public void SetTargetDirection(Vector3 d){
        dir = d;
    }

    public void OnEffect(WindEffect effect)
    {
        Vector3 diff = (sender.transform.position - effect.SourcePosition).normalized;
        diff = new Vector3(diff.x, 0, diff.z).normalized;
        dir = effect.Velocity.normalized;
        float a = Vector3.Angle(diff, dir);
        if (a <= homingThreshold) {
            isHoming = true;
            DeflectHoming();
        }else{
            Deflect();
        }
        isReflected = true;
        damage *= effect.ReflectDamageMultiplier;
    }
    void DeflectHoming(){
        dir = (sender.transform.position - transform.position).normalized;
        rb.velocity = dir * speed;
        transform.forward = dir;
    }

    void Deflect(){
        rb.velocity = dir * speed;
        transform.forward = dir.normalized;
    }

    public void SetSender(GameObject g){
        sender = g;
    }
}
