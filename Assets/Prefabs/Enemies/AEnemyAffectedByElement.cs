using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

public abstract class AEnemyAffectedByElement : AEnemy {
    
    [SerializeField] protected float _moveSpeedModifier;
    [SerializeField] protected float _baseMoveSpeed;
    [SerializeField] protected ElementalResistance elementalResistances;
 
    public void SetMoveSpeedModifier(float m){
        _moveSpeedModifier = m;
    }
    public void UpdateSpeed(){
        agent.speed = _baseMoveSpeed * _moveSpeedModifier;
    }
    public float GetFireResistance(){
        return Mathf.Clamp01(elementalResistances.fire);
    }
    public float GetIceResistance(){
        return Mathf.Clamp01(elementalResistances.ice);
    }
    public float GetWindResistance(){
        return Mathf.Clamp01(elementalResistances.wind);
    }
    public float GetLightningResistance(){
        return Mathf.Clamp01(elementalResistances.lightning);
    }
}