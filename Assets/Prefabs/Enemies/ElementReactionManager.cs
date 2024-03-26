using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.VFX;

// list of resistances against each element from 0 to 1, 0 being no resistance and 1 being total resistance
public struct ElementalResistance{
    public float fire;
    public float ice;
    public float wind;
    public float lightning;
}

public enum Element {
    fire = 1,
    ice = 2,
    wind = 3,
    lightning = 4
}

public class ElementReactionManager : NetworkBehaviour, IEffectListener<TemperatureEffect>, IEffectListener<WindEffect>
{
    [SerializeField] AEnemyAffectedByElement _aEnemy;
    [SerializeField] ParticleSystem _slowParticles;
    [SerializeField] VisualEffect _burnParticles;
    [SerializeField] float _freezeThreshold;
    [SerializeField] float _burnThreshold;
    [SerializeField] float _internalTemperature;
    [SerializeField] float _tempResetRate;
    [SerializeField] float _kbDuration;
    [SerializeField] float _stunDuration;
    [SerializeField] Rigidbody rb;
    [SerializeField] float _burnInterval;
    bool _isBurning;
    bool _isFrozen;
    float _burnTimer = 0;
    float _stunTimer;

    /*
    Update internal temperature
    */
    public void OnEffect(TemperatureEffect effect)
    {
        if(effect.TempDelta > 0){
            _internalTemperature += effect.TempDelta * (1 - _aEnemy.GetFireResistance());
            _aEnemy.TakeDamageWithElement(Mathf.Abs(effect.TempDelta), Element.fire);
        } else {
            _internalTemperature += effect.TempDelta * (1 - _aEnemy.GetIceResistance());
            _aEnemy.TakeDamageWithElement(Mathf.Abs(effect.TempDelta), Element.ice);
        }
    }
    
    /**
    * Knockback effect from wind
    */
    public void OnEffect(WindEffect effect){
        KnockBack(effect.Velocity * effect.KBMultiplier);
        _aEnemy.TakeDamageWithElement(effect.Velocity.magnitude, Element.wind);
    }

    void KnockBack(Vector3 dir){
        _aEnemy.GetAgent().enabled = false;
        GetComponent<Rigidbody>().AddForce(dir * (1 - _aEnemy.GetWindResistance()), ForceMode.Impulse);
        Invoke(nameof(ResetKnockBack), _kbDuration);
    }

    void ResetKnockBack(){ //if _stunDuration is 0, ResetStun will immediately be called and the agent will be reenabled
        _stunTimer = Time.time + _stunDuration;
    }

    void ResetStun(){
        _aEnemy.GetAgent().enabled = true;
    }

    void Burn(){
        if (_aEnemy.GetFireResistance() == 1) return;
        if(!_isBurning){
            _burnParticles.Play();
            _isBurning = true;
        }
        if(Time.time > _burnTimer){
            _aEnemy.TakeDamageWithElement(_internalTemperature * 0.01f, Element.fire);
            _burnTimer = Time.time + _burnInterval;
        }
    }

    void Slow(){
        _slowParticles.Play();
        _aEnemy.SetMoveSpeedModifier(Mathf.Clamp01(1 - ((1 - 1 / (-_internalTemperature)) * (1 - _aEnemy.GetIceResistance()))));
    }

    void Freeze(){
        // todo: add freeze shader
        if(_aEnemy.GetIceResistance() == 1) return;
        _aEnemy.GetAgent().enabled = false;
    }

    void Start(){
        _slowParticles.Stop();
        _burnParticles.Stop();
    }

    void UpdateTemperature(){
        if(_internalTemperature > _tempResetRate){
            _internalTemperature -= _tempResetRate * Time.deltaTime;
        }
        
        if (_internalTemperature < -_tempResetRate){
            _internalTemperature += _tempResetRate * Time.deltaTime;
            Slow();
        } else {
            _aEnemy.SetMoveSpeedModifier(1);
        }
        if(_internalTemperature > _burnThreshold){
            Burn();
            _isFrozen = false;
        } else {
            _burnParticles.Stop();
            _isBurning = false;
        }
        
        if (_internalTemperature < _freezeThreshold) {
            Freeze();
        }
        else{
            _isFrozen = false;
        }

        if(Time.time > _stunDuration){
            ResetStun();
        }
    }

    void Update(){
        _slowParticles.Stop();
        UpdateTemperature();

    }
}
