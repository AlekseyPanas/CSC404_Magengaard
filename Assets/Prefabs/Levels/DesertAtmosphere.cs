using System.Collections;
using System.Collections.Generic;
using FMODUnity;
using UnityEngine;

public class DesertAtmosphere : MonoBehaviour
{
    private StudioEventEmitter _emitter;
    
    void Start()
    {
        _emitter = GetComponent<StudioEventEmitter>();
    }
}
