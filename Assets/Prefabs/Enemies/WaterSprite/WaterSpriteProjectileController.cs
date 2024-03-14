using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class WaterSpriteProjectileController : NetworkBehaviour, IEffectListener<TemperatureEffect>
{
    public GameObject player;
    [SerializeField] float damage;
    [SerializeField] float stopTrackingRadius;
    [SerializeField] float speed;
    [SerializeField] Rigidbody rb;
    [SerializeField] float lifetime;
    [SerializeField] float temperature;
    [SerializeField] Collider col;
    Vector3 dir;
    bool stoppedTracking = false;
    void Start()
    {
        Destroy(gameObject, lifetime);
    }

    // Update is called once per frame
    void Update()
    {
        if (player != null){
            dir = player.transform.position - transform.position;
            float dis = dir.magnitude;
            if (dis > stopTrackingRadius && !stoppedTracking){
                //dir = new Vector3(dir.x, 0, dir.z).normalized;
                rb.velocity = dir.normalized * speed;
                transform.forward = dir;
            } else {
                stoppedTracking = true;
            }
        }
    }

    void OnTriggerEnter(Collider col){
        if(col.CompareTag("Player") || col.CompareTag("Ground") ){
            IEffectListener<DamageEffect>.SendEffect(col.gameObject, new DamageEffect(){Amount = (int)damage, SourcePosition = transform.position});
            IEffectListener<TemperatureEffect>.SendEffect(col.gameObject, new TemperatureEffect(){TempDelta = temperature, Collider = this.col});
            Destroy(gameObject);
        }
    }

    public void OnEffect(TemperatureEffect effect)
    {
        if (effect.TempDelta > 0) { // fire spell
            Destroy(gameObject);
        } 
    }
}
