using System;
using UnityEngine;

public class FireSpriteElementReactionManager : AElementReactionManager 
{
    [SerializeField] float _stunDuration;
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

    protected override void ResetKnockBack(){ //if _stunDuration is 0, ResetStun will immediately be called and the agent will be reenabled
        _stunTimer = Time.time + _stunDuration;
    }

    protected void ResetStun(){
        _aEnemy.GetAgent().enabled = true;
    }

    protected override void Update(){
        base.Update();
        if(Time.time > _stunTimer){
            ResetStun();
        }
    }
}