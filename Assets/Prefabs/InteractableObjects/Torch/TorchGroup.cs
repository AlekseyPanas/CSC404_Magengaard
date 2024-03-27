using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TorchGroup : MonoBehaviour
{
    public List<AToggleable> toggles;
    public float interval = 1.0f;

    private int _togglesLit;

    public void ToggleNext()
    {
        if (_togglesLit >= toggles.Count)
            return;
        
        toggles[_togglesLit].setToggle(true);
            
        _togglesLit++;

        Invoke(nameof(ToggleNext), interval);
    }

    private void OnTriggerEnter(Collider other)
    {
        Invoke(nameof(ToggleNext), interval);
    }
}
