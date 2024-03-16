using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyFireSpriteAnimationEvents : MonoBehaviour
{
    [SerializeField] EnemyFireSpriteController fireSpriteController;
    public void StartAttack(){
        fireSpriteController.AttackPlayer();
    } 

    public void EndAttack(){
        fireSpriteController.EndAttack();
    }
    public void OnSpawn(){
        fireSpriteController.OnSpawn();
    }
}
