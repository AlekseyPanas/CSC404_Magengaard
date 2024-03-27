using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TorchGroup : MonoBehaviour
{
    public List<AToggleable> toggles;
    public float interval = 1.0f;

    private int _togglesLit;

    private bool triggered = false;

    public bool manualTrigger = false;

    public void ToggleNext()
    {
        if (_togglesLit >= toggles.Count)
            return;
        
        toggles[_togglesLit].setToggle(true);
        
        _togglesLit++;

        Invoke(nameof(ToggleNext), interval);
    }

    public void Trigger()
    {
        if (triggered)
        {
            return;
        }

        triggered = true;
        
        ToggleNext();
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (!manualTrigger)
        {
            Invoke(nameof(ToggleNext), interval);
        }
    }
}
