using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterSpriteAnimationEvents : MonoBehaviour
{
    public EnemyWaterSpriteController waterSpriteController;

    public void OnAttack(){
        waterSpriteController.AttackPlayer();
    }
}
