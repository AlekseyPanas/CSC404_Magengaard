using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyFireSpriteAnimationEvents : MonoBehaviour
{
    [SerializeField] EnemyFireSpriteController fireSpriteController;
    public void OnAttack(){
        fireSpriteController.AttackPlayer();
    } 
}
