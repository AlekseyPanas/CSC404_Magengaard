using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class EnemyCactusAnimationEvents : NetworkBehaviour
{
    public EnemyCactusController ecc;

    public void AttackPlayerAnimEvent(){
        if(!IsServer) return;
        ecc.AttackPlayer();
        GetComponent<Animator>().SetBool("isShooting", false);
    }

    public void ResetSpeed(){
        ecc.ResetSpeed();
    }

    public void SlowSpeed(){
        ecc.SlowSpeed();
    }
}
