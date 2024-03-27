using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GodRayController : NetworkBehaviour
{
    [SerializeField] ParticleSystem ps;

    void Start(){
        ps.Stop();
    }
    void OnTriggerEnter(Collider col){
        if(col.CompareTag("Player")){
            ps.Play();
        }
    }

    void OnTriggerExit(Collider col){
        if(col.CompareTag("Player")){
            ps.Stop();
        }
    }
}
