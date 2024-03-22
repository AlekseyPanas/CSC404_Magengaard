using System;
using UnityEngine;

public abstract class ATerminal<T> : AActivatable, IEffectListener<T>, ITerminal
{
    public event Action OnCrystalPlaced;
    [SerializeField] private GameObject dormant;
    [SerializeField] private GameObject inactive;
    [SerializeField] private GameObject active;
    [SerializeField] private GameObject disabledVFX;
    [SerializeField] private GameObject activeVFX;
    [SerializeField] private Transform VFXSpawnPoint;
    bool isEnabled = true;
    
    public void UpdateState(){
        dormant.SetActive(state == ActiveState.DORMANT);
        inactive.SetActive(state == ActiveState.INACTIVE);
        active.SetActive(state == ActiveState.ACTIVE);
    }
    public void OnEffect(T effect)
    {
        if(!isEnabled) return;
        if(IsAboveThreshold(effect) && state == ActiveState.INACTIVE){
            SetStateActive();
            UpdateState();
            Instantiate(activeVFX, VFXSpawnPoint.transform.position, Quaternion.identity);
        }
    }

    public void ToggleDormant(bool isDormant){
        if(isDormant){
            state = ActiveState.DORMANT;
        } else {
            state = ActiveState.INACTIVE;
        }
        UpdateState();
    }
    public abstract bool IsAboveThreshold(T effect);

    public bool IsDormant(){
        return state == ActiveState.DORMANT;
    }

    public void PlaceCrystal(){
        ToggleDormant(false);
        OnCrystalPlaced?.Invoke();
    }

    public void DisableTerminal(){
        disabledVFX.GetComponent<DisabledVFXController>().EnableVFX();
        isEnabled = false;
    }
    public void EnableTerminal(){
        disabledVFX.GetComponent<DisabledVFXController>().DisableVFX();
        isEnabled = true;
    }
}
