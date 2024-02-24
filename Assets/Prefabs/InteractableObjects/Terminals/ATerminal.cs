using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ATerminal<T> : AActivatable, IEffectListener<T>
{
    [SerializeField] private GameObject dormant;
    [SerializeField] private GameObject inactive;
    [SerializeField] private GameObject active;
    [SerializeField] private Transform activeVFX;
    [SerializeField] private Transform VFXSpawnPoint;

    public void UpdateState(){
        dormant.SetActive(state == ActiveState.DORMANT);
        inactive.SetActive(state == ActiveState.INACTIVE);
        active.SetActive(state == ActiveState.ACTIVE);
    }
    public void OnEffect(T effect)
    {
        if(IsAboveThreshold(effect)){
            SetStateActive();
        }
        Instantiate(activeVFX, VFXSpawnPoint.transform.position, Quaternion.identity);
    }

    public void ToggleDormant(bool isDormant){
        if(isDormant){
            state = ActiveState.DORMANT;
        } else {
            state = ActiveState.INACTIVE;
        }
    }

    public abstract bool IsAboveThreshold(T effect);
}
