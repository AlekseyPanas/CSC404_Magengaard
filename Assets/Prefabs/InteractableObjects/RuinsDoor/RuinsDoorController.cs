using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class RuinsDoorController : NetworkBehaviour, IEffectListener<WindEffect>
{
    [SerializeField] Animator anim;
    public void OnEffect(WindEffect effect)
    {
        anim.SetTrigger("Open");
        GetComponent<Collider>().enabled = false;
    }
}
