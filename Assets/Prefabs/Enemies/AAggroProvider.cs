using System;
using UnityEngine;

public class AAggroProvider: MonoBehaviour
{
    public event Action<GameObject> AggroEvent;

    public void TriggerAggroEvent(GameObject g){
        AggroEvent?.Invoke(g);
    }
}
