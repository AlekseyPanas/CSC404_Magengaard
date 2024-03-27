using System;
using UnityEngine;

public abstract class AEnemyAffectedByElement : AEnemy {
    
    [SerializeField] protected float _moveSpeedModifier;
    [SerializeField] protected float _baseMoveSpeed;
    [SerializeField] protected ElementalResistance elementalResistances;
    protected bool _isAlive = true;
 
    public void SetMoveSpeedModifier(float m){
        _moveSpeedModifier = m;
    }
    public void UpdateSpeed(){
        agent.speed = _baseMoveSpeed * _moveSpeedModifier;
    }
    public float GetFireResistance(){
        return Mathf.Clamp(elementalResistances.fire, -1, 2);
    }
    public float GetIceResistance(){
        return Mathf.Clamp(elementalResistances.ice, -1, 2);
    }
    public float GetWindResistance(){
        return Mathf.Clamp(elementalResistances.wind, -1, 2);
    }
    public float GetLightningResistance(){
        return Mathf.Clamp(elementalResistances.lightning, -1, 2);
    }
    public float GetImpactResistance(){
        return Mathf.Clamp(elementalResistances.impact, -1, 2);
    }
    public void TakeDamageWithElement(float amount, Element e){
        if(!_isAlive) return;
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
            case Element.impact:
                TakeDamage(amount * (1 - elementalResistances.impact));
            break;
        }
    }

    public void PauseAnimator(){
        anim.speed = 0;
    }
    public void ResumeAnimator(){
        anim.speed = 1;
    }
    
    public bool IsAlive(){
        return _isAlive;
    }
}