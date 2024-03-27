using System;
using UnityEngine;

public class FireSpriteElementReactionManager : AElementReactionManager 
{
    [SerializeField] float _stunDuration;
    [SerializeField] ParticleSystem _stunParticles;
    float _stunTimer;
    protected override void Start(){}
    protected override void UpdateTemperature()
    {
        if(_internalTemperature > _tempResetRate){
            _internalTemperature -= _tempResetRate * Time.deltaTime;
        }
        
        if (_internalTemperature < -_tempResetRate){
            _internalTemperature += _tempResetRate * Time.deltaTime;
        } else {
            _aEnemy.SetMoveSpeedModifier(1);
        }
    }
    protected override void KnockBack(Vector3 dir){
        _aEnemy.GetAgent().enabled = false;
        rb.AddForce(dir * (1 - _aEnemy.GetWindResistance()), ForceMode.Impulse);
        Invoke(nameof(ResetKnockBack), _kbDuration);
        _stunTimer = Time.time + _stunDuration + _kbDuration;
        _aEnemy.PauseAnimator();
    }

    protected override void ResetKnockBack(){ //if _stunDuration is 0, ResetStun will immediately be called and the agent will be reenabled
        rb.velocity = Vector3.zero;
        _stunParticles.Play();
    }

    protected void ResetStun(){
        _aEnemy.GetAgent().enabled = true;
        _stunParticles.Stop();
        _aEnemy.ResumeAnimator();
    }

    protected override void Update(){
        base.Update();
        if(Time.time > _stunTimer){
            ResetStun();
        }
    }
}