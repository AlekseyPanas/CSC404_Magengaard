using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCombatManager : MonoBehaviour, IAggroable
{
    public static PlayerCombatManager instance;
    [SerializeField] private List<GameObject> currentlyAgrod;
    [SerializeField] private bool isInCombat;
    public delegate void EnterCombat();
    public static EnterCombat enterCombat;
    public delegate void ExitCombat();
    public static EnterCombat exitCombat;

    void Awake(){
        if (instance != null && instance != this) { 
            Destroy(this); 
        } 
        else { 
            instance = this; 
        }
    }
    void Start()
    {
        currentlyAgrod = new List<GameObject>();
    }

    public void Aggro(GameObject who) {
        currentlyAgrod.Add(who);
        isInCombat = true;
        enterCombat?.Invoke();
    }

    public void DeAggro(GameObject who) {
        currentlyAgrod.Remove(who);
        if (currentlyAgrod.Count == 0){
            isInCombat = false;
            exitCombat?.Invoke();
        }
    }
}
