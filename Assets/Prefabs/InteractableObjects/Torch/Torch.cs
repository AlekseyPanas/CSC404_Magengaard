using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Torch : AToggleable, IEffectListener<TemperatureEffect>, IEffectListener<LightningEffect>, IEffectListener<WaterEffect>, IEffectListener<WindEffect>
{
    [SerializeField] float activateTempThreshold;
    [SerializeField] float activateVoltageThreshold;
    [SerializeField] float deactivateTempThreshold;
    [SerializeField] float deactivateWaterThreshold;
    [SerializeField] float deactivateWindThreshold;
    [SerializeField] GameObject fireVFX;
    [SerializeField] Light pointLight;

    void Awake(){
        changedToggleEvent += ToggleFire;
        ToggleFire(false);
    }

    new void OnDestroy(){
        changedToggleEvent -= ToggleFire;
        base.OnDestroy();
    }
    public void OnEffect(TemperatureEffect effect)
    {
        if (IsTempAboveThreshold(effect)) {
            setToggle(true);
        } else if (IsTempBelowThreshold(effect)) {
            setToggle(true);
        }
    }

    public void OnEffect(LightningEffect effect)
    {
        if (IsVoltageAboveThreshold(effect)) {
            setToggle(true);
        }
    }

    public void OnEffect(WaterEffect effect)
    {
        if (IsWaterAboveThreshold(effect)) {
            setToggle(false);
        }
    }

    public void OnEffect(WindEffect effect)
    {
        if (IsWindAboveThreshold(effect)) {
            setToggle(false);
        }
    }

    bool IsTempAboveThreshold(TemperatureEffect e){
        return e.TempDelta >= activateTempThreshold;
    }
    bool IsTempBelowThreshold(TemperatureEffect e){
        return e.TempDelta <= deactivateTempThreshold;
    }
    
    bool IsVoltageAboveThreshold(LightningEffect e){
        return e.Voltage >= activateVoltageThreshold;
    }

    bool IsWaterAboveThreshold(WaterEffect e){
        return e.WaterVolume >= deactivateWaterThreshold;
    }

    bool IsWindAboveThreshold(WindEffect e){
        return e.Velocity.magnitude >= deactivateWaterThreshold;
    }

    void ToggleFire(bool fireIsOn){
        fireVFX.SetActive(fireIsOn);
        pointLight.enabled = fireIsOn;
    }
}
