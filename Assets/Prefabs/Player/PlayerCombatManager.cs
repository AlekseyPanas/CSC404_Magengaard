using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCombatManager : MonoBehaviour, IAggroable
{
    public static PlayerCombatManager instance;
    [SerializeField] private List<GameObject> currentlyAgrod;
    public static Action OnEnterCombat;
    public static Action OnExitCombat;
    private bool isInCombat = false;

    void Awake(){
        if (instance != null && instance != this) { 
            Destroy(this); 
        } 
        else { 
            instance = this; 
        }
    }

    void Start() {
        currentlyAgrod = new List<GameObject>();
    }

    public void Aggro(GameObject who) {
        currentlyAgrod.Add(who);
        if (!isInCombat) {
            isInCombat = true;
            OnEnterCombat?.Invoke();
        }
    }

    public void DeAggro(GameObject who) {
        currentlyAgrod.Remove(who);
        if (currentlyAgrod.Count == 0){
            isInCombat = false;
            OnExitCombat?.Invoke();
        }
    }
}
