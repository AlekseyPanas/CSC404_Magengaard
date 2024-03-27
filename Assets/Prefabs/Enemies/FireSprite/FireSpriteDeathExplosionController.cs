using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class FireSpriteDeathExplosionController : NetworkBehaviour
{
    [SerializeField] float lifetime;
    [SerializeField] float damage;
    [SerializeField] float temperature;
    [SerializeField] int numFramesDestroy;
    [SerializeField] GameObject _fireMesh;
    int frameCounter;

    void Start(){
        Destroy(gameObject, lifetime);
    }

    void OnTriggerEnter(Collider col){
        IEffectListener<ImpactEffect>.SendEffect(col.gameObject, new ImpactEffect(){Amount = (int)damage, Direction = col.transform.position - transform.position});
        IEffectListener<TemperatureEffect>.SendEffect(col.gameObject, new TemperatureEffect{TempDelta = temperature, mesh = _fireMesh, 
            Direction = col.transform.position - transform.position, IsAttack = true});
    }

    void Update()
    {
        if(frameCounter > numFramesDestroy){
            GetComponent<Collider>().enabled = false;
        }
        frameCounter++;
    }
}
