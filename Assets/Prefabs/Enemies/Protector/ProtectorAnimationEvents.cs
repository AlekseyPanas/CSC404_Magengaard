using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProtectorAnimationEvents : MonoBehaviour
{
    public EnemyProtectorController controller;
    public Collider col;
    public void OnActivate(){
        controller.OnActivate();
    }

    public void EnableCol(){
        col.enabled = true;
    }

    public void DisableCol(){
        col.enabled = false;
        controller.ClearCollided();
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
