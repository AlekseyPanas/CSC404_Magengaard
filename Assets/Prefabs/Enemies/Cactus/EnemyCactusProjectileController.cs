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
    bool isDeflected = false;
    bool isHoming = false;
    void Start()
    {
        rb.velocity = dir * speed;
        Destroy(gameObject, lifetime);
        transform.forward = dir.normalized;
        collided = new List<GameObject>();
    }

    void OnTriggerEnter(Collider col){
        if(!isDeflected && col.gameObject == sender) return;
        if(!collided.Contains(col.gameObject)){
            if((!isDeflected && col.CompareTag("Player")) || (isDeflected && col.CompareTag("Enemy"))){
                IEffectListener<ImpactEffect>.SendEffect(col.gameObject, new ImpactEffect { Amount = (int)damage, Direction = col.transform.position - transform.position});
            }
            if(col.CompareTag("Ground") || (!isDeflected && col.CompareTag("Player")) || (isDeflected && col.CompareTag("Enemy"))){
                Destroy(gameObject);
            }
            collided.Add(col.gameObject);
        }
    }

    void Update(){
        model.Rotate(0, 0, rotationSpeed);
        if(isDeflected && isHoming){
            DeflectHoming();
        }
    }

    public void SetTargetDirection(Vector3 d){
        dir = d;
    }

    public void OnEffect(WindEffect effect)
    {
        if (isDeflected) return;
        Vector3 diff = new Vector3(effect.Direction.x, 0, effect.Direction.z).normalized;
        diff = new Vector3(diff.x, 0, diff.z).normalized;
        dir = effect.Velocity.normalized;
        float a = Vector3.Angle(diff, dir);
        if (a <= homingThreshold) {
            isHoming = true;
            DeflectHoming();
        }else{
            Deflect();
        }
        if(!isDeflected && effect.DeflectionParticle != null){
            GameObject ps = Instantiate(effect.DeflectionParticle, transform.position, Quaternion.identity);
            ps.transform.forward = dir + new Vector3(0,-1,0);
        }
        isDeflected = true;
        damage *= effect.ReflectDamageMultiplier;
    }
    void DeflectHoming(){
        if(sender == null) return;
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
