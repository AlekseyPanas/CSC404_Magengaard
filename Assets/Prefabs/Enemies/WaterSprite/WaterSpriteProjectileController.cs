using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System.Runtime.InteropServices;

public class WaterSpriteProjectileController : NetworkBehaviour, IEffectListener<TemperatureEffect>, IEffectListener<WindEffect>
{
    public GameObject player;
    [SerializeField] GameObject shardModel;
    [SerializeField] GameObject shardMesh;
    [SerializeField] float damage;
    [SerializeField] float stopTrackingRadius;
    [SerializeField] float speed;
    [SerializeField] Rigidbody rb;
    [SerializeField] float lifetime;
    [SerializeField] float temperature;
    [SerializeField] Collider col;
    [SerializeField] private GameObject sender;
    [SerializeField] private float homingThreshold;
    bool init = false;
    bool isHoming;
    bool isDeflected;
    List<GameObject> collided = new();
    Vector3 dir;
    bool stoppedTracking = false;
    void Start()
    {
        Destroy(gameObject, lifetime);
    }

    // Update is called once per frame
    void Update()
    {
        if (isDeflected){
            if(isHoming) DeflectHoming();
        } else if (player != null){
            dir = player.transform.position - transform.position;
            float dis = dir.magnitude;
            if ((dis > stopTrackingRadius && !stoppedTracking) || !init){
                rb.velocity = dir.normalized * speed;
                transform.forward = dir;
                init = true;
            } else {
                stoppedTracking = true;
            }
            
        }
        shardModel.transform.Rotate(new Vector3(0,0,5f));
    }

    void OnTriggerEnter(Collider col){
        if(col.gameObject == sender && !isDeflected) return;

        if(!collided.Contains(col.gameObject)){
            if((!isDeflected && col.CompareTag("Player")) || (isDeflected && col.CompareTag("Enemy"))){
                IEffectListener<ImpactEffect>.SendEffect(col.gameObject, new ImpactEffect(){Amount = (int)damage, Direction = col.transform.position - transform.position});
                IEffectListener<TemperatureEffect>.SendEffect(col.gameObject, new TemperatureEffect(){TempDelta = temperature, mesh = shardMesh, 
                Direction = col.transform.position - transform.position, IsAttack = true});
            }
            if(col.CompareTag("Ground") || (!isDeflected && col.CompareTag("Player")) || (isDeflected && col.CompareTag("Enemy"))){
                Destroy(gameObject);
            }
            collided.Add(col.gameObject);
        }
    }

    public void OnEffect(TemperatureEffect effect)
    {
        if (effect.TempDelta > 0) {
            Destroy(gameObject);
        }
    }

    public void OnEffect(WindEffect effect)
    {
        if(isDeflected) return;
        Vector3 diff = new Vector3(effect.Direction.x, 0, effect.Direction.z).normalized;
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
