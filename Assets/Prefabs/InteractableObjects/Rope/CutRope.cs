using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CutRope : MonoBehaviour, IEffectListener<DamageEffect>
{
    private float health = 15.0f;
    private bool dead;

    public HingeJoint joint;

    private void Death()
    {
        dead = true;

        if (joint != null)
        {
            Destroy(joint);
            joint = null;
        }
    }
    
    public void OnEffect(DamageEffect effect)
    {
        if (dead)
        {
            return;
        }
        
        health -= effect.Amount;

        if (health <= 0)
        {
            Death();
        }
    }
}
