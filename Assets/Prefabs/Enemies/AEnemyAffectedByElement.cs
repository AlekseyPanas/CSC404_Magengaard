using System;
using UnityEngine;

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
    public void TakeDamageWithElement(float amount, Element e){
        switch (e){
            case Element.fire:
                TakeDamage(amount * (1 - elementalResistances.fire));
            break;
            case Element.ice:
                TakeDamage(amount * (1 - elementalResistances.ice));
            break;
            case Element.wind:
                TakeDamage(amount * (1 - elementalResistances.wind));
            break;
            case Element.lightning:
                TakeDamage(amount * (1 - elementalResistances.lightning));
            break;
        }
    }
}