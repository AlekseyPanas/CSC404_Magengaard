using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class WaterSpriteProjectileController : NetworkBehaviour, IEffectListener<TemperatureEffect>
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
    bool init = false;

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
            if ((dis > stopTrackingRadius && !stoppedTracking) || !init){
                //dir = new Vector3(dir.x, 0, dir.z).normalized;
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
        if((col.CompareTag("Player") || col.CompareTag("Ground")) && !col.CompareTag("Enemy")){
            IEffectListener<ImpactEffect>.SendEffect(col.gameObject, new ImpactEffect(){Amount = (int)damage, SourcePosition = transform.position});
            IEffectListener<TemperatureEffect>.SendEffect(col.gameObject, new TemperatureEffect(){TempDelta = temperature, mesh = shardMesh});
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
