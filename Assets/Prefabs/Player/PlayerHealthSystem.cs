using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthSystem : NetworkBehaviour, IEffectListener<DamageEffect>
{
    [SerializeField] float maxHP;
    [SerializeField] float currHP;
    [SerializeField] Animator anim;
    public delegate void OnTakeDamage(PlayerHealthSystem playerHealthSystem);
    public delegate void OnDeath();
    public static OnTakeDamage onTakedamage;
    public static OnDeath onDeath;
    Vector3 damageDir;
    public void OnEffect(DamageEffect effect)
    {
        if (currHP <= 0) return;

        currHP -= effect.Amount;
        onTakedamage(this);
        damageDir = effect.SourcePosition - transform.position;
        if(currHP <= 0){
            onDeath();
        }
    }
    void Start()
    {
        currHP = maxHP;
        onDeath += Die;
    }
    void Die(){
        damageDir = new Vector3(damageDir.x, 0, damageDir.z);
        float[] angles = {  Vector3.Angle(damageDir, transform.forward),    // fall forward death2
                            Vector3.Angle(damageDir, -transform.forward),   // fall backwards death1
                            Vector3.Angle(damageDir, transform.right),      // fall right death4
                            Vector3.Angle(damageDir, -transform.right) };   // fall left death3
        int i = Array.IndexOf(angles, angles.Min());
        Debug.Log(i);
        anim.SetInteger("DeathDir", i);
        anim.SetTrigger("Die");
    }
    public float GetHPPercent(){
        return currHP/maxHP;
    }
}
