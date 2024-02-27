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
    [SerializeField] GameObject respawnVFX;
    [SerializeField] GameObject playerModel;
    public delegate void OnTakeDamage(PlayerHealthSystem playerHealthSystem);
    public delegate void OnDeath();
    public delegate void OnRespawn();
    public delegate void OnRespawnFinished();
    public static OnTakeDamage onTakedamage;
    public static OnDeath onDeath;
    public static OnRespawn onRespawn;
    public static OnRespawnFinished onRespawnFinished;
    Vector3 damageDir;
    GameObject respawnPoint;
    public void OnEffect(DamageEffect effect)
    {
        if (currHP <= 0) return;

        currHP -= effect.Amount;
        onTakedamage(this);
        damageDir = transform.position - effect.SourcePosition;
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
        float[] angles = {  Vector3.Angle(damageDir, -transform.forward),    // fall forward death2
                            Vector3.Angle(damageDir, transform.forward),   // fall backwards death1
                            Vector3.Angle(damageDir, -transform.right),      // fall right death4
                            Vector3.Angle(damageDir, transform.right) };   // fall left death3
        int i = Array.IndexOf(angles, angles.Min());
        anim.SetInteger("DeathDir", i);
        anim.SetTrigger("Die");
        Invoke("StartRespawn", 3f);
    }
    public float GetHPPercent(){
        return currHP/maxHP;
    }

    void StartRespawn(){
        GameObject[] respawnPoints = GameObject.FindGameObjectsWithTag("Respawn");
        int num = -1;
        respawnPoint = respawnPoints[0];
        foreach (GameObject r in respawnPoints) {
            RespawnPoint rp = r.GetComponent<RespawnPoint>();
            if (rp.isActive) {
                int currNum = rp.number;
                if (currNum > num) {
                    num = currNum;
                    respawnPoint = r;
                }
            }
        }
        playerModel.SetActive(false);
        Invoke("Respawn", 2);
    }

    void Respawn(){
        onRespawn();
        GetComponent<CharacterController>().enabled = false;
        transform.position = respawnPoint.transform.position;
        GetComponent<CharacterController>().enabled = true;
        Invoke("ShowRespawnVFX", 1f);
        Invoke("ShowPlayer",2f);
    }
    void ShowRespawnVFX(){
        Instantiate(respawnVFX, respawnPoint.transform.position + new Vector3(0,-2f,0), Quaternion.Euler(new Vector3(-90,0,0)));
    }
    void ShowPlayer(){
        onRespawnFinished();
        playerModel.SetActive(true);
        anim.Play("rig_Idle");
        currHP = maxHP;
    }
}
