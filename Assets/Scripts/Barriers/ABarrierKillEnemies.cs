using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public abstract class ABarrierKillEnemies : NetworkBehaviour
{
    [SerializeField] private List<GameObject> enemiesList;
    void Start() {
        foreach(GameObject enemy in enemiesList){
            if(enemy.TryGetComponent<IEnemy>(out var i)){
                i.OnEnemyDeath += OnEnemyDeath;
            } else {
                Debug.Log(name + " ABarrierKillEnemies: attempted to inject an object that isn't of type IEnemy");
            }
        }
    }

    void OnEnemyDeath(GameObject target){
        enemiesList.Remove(target);
        if(enemiesList.Count == 0){
            BarrierDisable();
        }
    }
    protected abstract void BarrierDisable();
    protected abstract void BarrierEnable();
}
