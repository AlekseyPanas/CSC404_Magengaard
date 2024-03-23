using System;
using UnityEngine;

public abstract class ATerminal<T> : AActivatable, IEffectListener<T>, ITerminal
{
    public event Action OnCrystalPlaced;
    [SerializeField] private GameObject dormant;
    [SerializeField] private GameObject inactive;
    [SerializeField] private GameObject active;
    [SerializeField] private GameObject activeVFX;
    [SerializeField] private Transform VFXSpawnPoint;
    [SerializeField] private GameObject crystal;
    [SerializeField] private GameObject blinkCrystal;
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
            if(crystal!=null) crystal.SetActive(false);
        }
        UpdateState();
    }
    public abstract bool IsAboveThreshold(T effect);

    public bool IsDormant(){
        return state == ActiveState.DORMANT;
    }

    public void PlaceCrystal(GameObject g){
        OnCrystalPlaced?.Invoke();
        crystal = g;
        crystal.transform.parent = transform;
        blinkCrystal.SetActive(false);
        crystal.GetComponent<CrystalController>().StopPS();
    }
}
