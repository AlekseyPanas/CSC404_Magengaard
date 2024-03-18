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
        IEffectListener<DamageEffect>.SendEffect(col.gameObject, new DamageEffect(){Amount = (int)damage, SourcePosition = transform.position});
        IEffectListener<TemperatureEffect>.SendEffect(col.gameObject, new TemperatureEffect{TempDelta = temperature, mesh = _fireMesh});
    }

    void Update()
    {
        if(frameCounter > numFramesDestroy){
            GetComponent<Collider>().enabled = false;
        }
        frameCounter++;
    }
}
