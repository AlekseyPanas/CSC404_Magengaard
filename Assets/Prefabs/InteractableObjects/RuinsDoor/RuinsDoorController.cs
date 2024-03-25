using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using FMODUnity;

public class RuinsDoorController : NetworkBehaviour, IEffectListener<WindEffect>
{
    [SerializeField] Animator anim;
    private StudioEventEmitter _emitter;
    bool _hasPlayedSound = false;

    void Awake() {
        _emitter = GetComponent<StudioEventEmitter>();
    }

    public void OnEffect(WindEffect effect)
    {
        anim.SetTrigger("Open");
        GetComponent<Collider>().enabled = false;
    }

    void OnCollisionEnter(Collision col) {
        if (col.gameObject.tag == "Player" && !_hasPlayedSound) {
            _hasPlayedSound = true;
            _emitter.Play();
        }
    }
}
