using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class FireSpriteDeathExplosionController : NetworkBehaviour
{
    [SerializeField] float lifetime;
    [SerializeField] float damage;
    [SerializeField] int numFramesDestroy;
    int frameCounter;

    void Start(){
        Destroy(gameObject, lifetime);
    }

    void OnTriggerEnter(Collider col){
        IEffectListener<DamageEffect>.SendEffect(col.gameObject, new DamageEffect(){Amount = (int)damage});
    }

    void Update()
    {
        if(frameCounter > numFramesDestroy){
            GetComponent<Collider>().enabled = false;
        }
        frameCounter++;
    }
}
