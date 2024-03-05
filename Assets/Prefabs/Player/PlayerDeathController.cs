using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class PlayerDeathController : NetworkBehaviour
{
    [SerializeField] Animator anim;
    [SerializeField] GameObject respawnVFX;
    [SerializeField] GameObject playerModel;
    [SerializeField] GestureSystem gs;
    [SerializeField] PlayerHealthSystem healthSystem;
    public delegate void OnDeath();
    public delegate void OnRespawn();
    public delegate void OnRespawnFinished();
    public static OnDeath onDeath = delegate{};
    public static OnRespawn onRespawn;
    public static OnRespawnFinished onRespawnFinished;
    AControllable<PlayerHealthSystem>.ControllerRegistrant healthSystemRegistrant;
    Vector3 damageDir;
    GameObject respawnPoint;
    Camera cam;
    CameraManager cameraManager;
    void Start()
    {
        cam = Camera.main;
        cameraManager = cam.GetComponent<CameraManager>();
        gs = FindAnyObjectByType<GestureSystem>().GetComponent<GestureSystem>();
        PlayerHealthSystem.OnHealthPercentChange += Die;
    }
    void Die(float percent, Vector3 dir){
        if (percent > 0) return;

        
        
        
        onDeath();
        damageDir = new Vector3(damageDir.x, 0, damageDir.z);
        float[] angles = {  Vector3.Angle(damageDir, -transform.forward),    // fall forward death2
                            Vector3.Angle(damageDir, transform.forward),   // fall backwards death1
                            Vector3.Angle(damageDir, -transform.right),      // fall right death4
                            Vector3.Angle(damageDir, transform.right) };   // fall left death3
        int i = Array.IndexOf(angles, angles.Min());
        anim.SetInteger("DeathDir", i);
        anim.SetTrigger("Die");
        Invoke("StartRespawn", 3f);
        GetComponent<Collider>().enabled = false;
        gs.disableGestureDrawing();
    }

    void TryRegister(){
        healthSystemRegistrant = healthSystem.RegisterController(10); //

        if (healthSystemRegistrant == null){
            healthSystemRegistrant?.DeRegister();
        }
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
        GetComponent<CharacterController>().enabled = false;
        transform.position = respawnPoint.transform.position;
        GetComponent<CharacterController>().enabled = true;
        cameraManager.AddOverrideFollow(new CameraFollowFixed(transform.position, transform.forward, 0.1f));
        Invoke("Respawn", 3);
        Invoke("EnablePlayerCollider", 0.1f);
    }

    void EnablePlayerCollider(){
        cameraManager.ClearOverrideFollow();
        GetComponent<Collider>().enabled = true;
    }

    void Respawn(){
        onRespawn();
        Invoke("ShowRespawnVFX", 1f);
        Invoke("ShowPlayer",2f);
    }
    void ShowRespawnVFX(){
        Instantiate(respawnVFX, respawnPoint.transform.position + new Vector3(0,-3f,0), Quaternion.Euler(new Vector3(-90,0,0)));
    }
    void ShowPlayer(){
        onRespawnFinished();
        playerModel.SetActive(true);
        anim.Play("rig_Idle");
        gs.enableGestureDrawing();
        healthSystem.ResetHP();
    }
}
