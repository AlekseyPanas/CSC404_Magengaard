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
    bool _isBurning;
    bool _isFrozen;

    /*
    Update internal temperature
    */
    public void OnEffect(TemperatureEffect effect)
    {
        _internalTemperature += effect.TempDelta;
    }
    
    /**
    * Knockback effect from wind
    */
    public void OnEffect(WindEffect effect){
        KnockBack(effect.Velocity);
    }

    void KnockBack(Vector3 dir){
        Debug.Log("knocking back");
        _aEnemy.GetAgent().enabled = false;
        GetComponent<Rigidbody>().AddForce(dir * (1 - _aEnemy.GetWindResistance()), ForceMode.Impulse);
        Invoke(nameof(ResetKnockBack), _kbDuration);
    }

    void ResetKnockBack(){
        Invoke(nameof(ResetStun), _stunDuration);
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
    }

    void Slow(){

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

    void Update(){
        _slowParticles.Stop();

        if(_internalTemperature > _tempResetRate){
            _internalTemperature -= _tempResetRate * Time.deltaTime;
        }
        
        if (_internalTemperature < -_tempResetRate){
            _internalTemperature += _tempResetRate * Time.deltaTime;
            _slowParticles.Play();
            _aEnemy.SetMoveSpeedModifier(Mathf.Clamp01(1 - ((1 - 1 / (-_internalTemperature)) * (1 - _aEnemy.GetIceResistance()))));
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
    }
}
