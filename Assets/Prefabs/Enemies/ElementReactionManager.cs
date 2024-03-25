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

public class ElementReactionManager : NetworkBehaviour, IEffectListener<TemperatureEffect>
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

    /*
    Update internal temperature
    */
    public void OnEffect(TemperatureEffect effect)
    {
        _internalTemperature += effect.TempDelta;
        Debug.Log(_internalTemperature);
    }
    
    /**
    * Knockback effect from wind
    */
    public void OnEffect(WindEffect effect){
        KnockBack(effect.Velocity);
    }

    void KnockBack(Vector3 dir){
        _aEnemy.GetAgent().enabled = false;
        GetComponent<Rigidbody>().AddForce(dir * (1 - _aEnemy.GetWindResistance()), ForceMode.Impulse);
        Invoke("ResetKnockBack", _kbDuration);
    }

    void ResetKnockBack(){
        GetComponent<Rigidbody>().velocity = Vector3.zero;
        Invoke(nameof(ResetStun), _stunDuration);
    }

    void ResetStun(){
        _aEnemy.GetAgent().enabled = true;
    }

    void Burn(){
        if (_aEnemy.GetFireResistance() == 1) return;
        _burnParticles.Play();

    }

    void Slow(){

    }

    void Freeze(){
        //freeze shader
        if(_aEnemy.GetIceResistance() == 1) return;
        _aEnemy.GetAgent().enabled = false;
    }

    void Start(){
        _slowParticles.Stop();
        _burnParticles.Stop();
    }

    void Update(){
        if(_internalTemperature > _tempResetRate){
            _internalTemperature -= _tempResetRate * Time.deltaTime;
        } else if (_internalTemperature < -_tempResetRate){
            _internalTemperature += _tempResetRate * Time.deltaTime;
            _slowParticles.Play();
            _aEnemy.SetMoveSpeedModifier(Mathf.Clamp01(1 - ((1 - 1 / (-_internalTemperature)) * (1 - _aEnemy.GetIceResistance()))));
        }
        _slowParticles.Stop();
        _burnParticles.Stop();
        _aEnemy.GetAgent().enabled = true;
        if(_internalTemperature > _burnThreshold){
            Burn();
        } else if (_internalTemperature < _freezeThreshold) {
            Freeze();
        }
        else{
            _aEnemy.SetMoveSpeedModifier(1);
        }
    }
}
