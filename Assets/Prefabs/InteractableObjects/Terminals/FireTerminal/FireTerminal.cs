using UnityEngine;
using System;
using Unity.Netcode;

public class FireTerminal : ATerminal<TemperatureEffect>
{
    [SerializeField] private float threshold;
    public override bool IsAboveThreshold(TemperatureEffect effect)
    {
        if (effect.TempDelta >= threshold){
            return true;
        }
        return false;
    }

    void Start(){
        UpdateState();
    }
}