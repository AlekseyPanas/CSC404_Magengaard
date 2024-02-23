using UnityEngine;
using System;
using Unity.Netcode;

public class IceTerminal : ATerminal<TemperatureEffect>
{
    [SerializeField] private float threshold;
    public override bool IsAboveThreshold(TemperatureEffect effect)
    {
        if (effect.TempDelta <= threshold){
            return true;
        }
        return false;
    }
}