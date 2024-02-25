using UnityEngine;
using System;
using Unity.Netcode;

public class IceTerminal : ATerminal<TemperatureEffect>
{
    [SerializeField] private float threshold;
    public override bool IsAboveThreshold(TemperatureEffect effect)
    {
        return effect.TempDelta <= threshold;
    }

    void Start(){
        UpdateState();
    }
}