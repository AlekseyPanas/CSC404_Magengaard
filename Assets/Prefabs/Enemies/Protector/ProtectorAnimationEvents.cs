using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProtectorAnimationEvents : MonoBehaviour
{
    public EnemyProtectorController controller;
    public Collider col;
    public ParticleSystem swordTrail;
    public ParticleSystem swordGlow;

    public void OnActivate(){
        controller.OnActivate();
    }

    public void StartSwordGlow(){
        swordGlow.Play();
    }
    public void StopSwordGlow(){
        swordGlow.Stop();
    }

    public void EnableCol(){
        col.enabled = true;
        swordTrail.Play();
    }

    public void DisableCol(){
        col.enabled = false;
        controller.ClearCollided();
        swordTrail.Stop();
    }

    public void EndAttack(){
        controller.EndAttack();
    }

    public void GroundAttack(){
        controller.GroundAttack();
    }

    public void Dash(){
        controller.StartDash();
    }
    public void FacePlayer(){
        controller.FacePlayer();
    }
}
