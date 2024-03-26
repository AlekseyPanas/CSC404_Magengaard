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
    bool isReflected;
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
        if (isReflected){
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
        if(col.gameObject == sender && !isReflected) return;

        if(!collided.Contains(col.gameObject)){
            if((!isReflected && col.CompareTag("Player")) || (isReflected && col.CompareTag("Enemy"))){
                IEffectListener<ImpactEffect>.SendEffect(col.gameObject, new ImpactEffect(){Amount = (int)damage, SourcePosition = transform.position});
                IEffectListener<TemperatureEffect>.SendEffect(col.gameObject, new TemperatureEffect(){TempDelta = temperature, mesh = shardMesh});
            }
            if(col.CompareTag("Ground") || (!isReflected && col.CompareTag("Player")) || (isReflected && col.CompareTag("Enemy"))){
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
        Debug.Log("deflected");
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
