using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AEnemyPlayerDetector : AEnemy
{
    [SerializeField] protected PlayerDetector playerDetector;
    public void OnSpawnViaSpawner(PlayerDetector pd){
        playerDetector = pd;
        GameObject player = pd.GetPlayer();
        if(player!= null) {
            TryAggro(player);
        }
    }
}
