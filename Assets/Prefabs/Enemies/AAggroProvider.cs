using System;
using UnityEngine;

public class AAggroProvider: MonoBehaviour
{
    public event Action<GameObject> AggroEvent;

    public void TriggerAggroEvent(GameObject g){
        Debug.Log(AggroEvent == null);
        Debug.Log("triggering agro event");
        AggroEvent?.Invoke(g);
    }
}
