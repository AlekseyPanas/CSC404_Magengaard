using System;
using UnityEngine;


public enum HealthControllablePriorities {
    DEATH = 1
}


/**
* Controllable which exposes direct manipulation of the player's health value 
*/
public class PlayerHealthControllable : AControllable<PlayerHealthControllable, ControllerRegistrant>
{
    public static event Action<float, Vector3> OnHealthPercentChange = delegate{};

    [SerializeField] float maxHP;
    [SerializeField] float currHP;
    
    /** 
    * Increase the player's current health by the absolute value of the given amount 
    */
    public void Heal(float amount) { 
        currHP = Math.Min(maxHP, currHP + Math.Abs(amount)); 
        OnHealthPercentChange(currHP/maxHP, Vector3.up);
    }

    /** 
    * Decreases the player's current health by the absolute value of the given amount, 
    * and provides details regarding where the damage was taken from
    */
    public void Damage(float amount, Vector3 direction) { 
        currHP = Math.Max(0f, currHP - Math.Abs(amount));
        OnHealthPercentChange(currHP/maxHP, direction); 
    }

    /** 
    * Decreases the player's current health by the absolute value of the given amount
    */
    public void Damage(float amount) { Damage(amount, Vector3.up); }

    /** 
    * Sets current health to the specified percentage of max health
    */
    public void SetHPPercent(float percent) { 
        currHP = Math.Clamp(percent, 0f, 1f) * maxHP; 
        OnHealthPercentChange(currHP/maxHP, Vector3.up);
    }

    /** 
    * Refills health back to the maximum
    */
    public void ResetHP() { 
        currHP = maxHP; 
        OnHealthPercentChange(currHP/maxHP, Vector3.up);
    }

    protected override PlayerHealthControllable ReturnSelf() { return this; }

    void Start() { currHP = maxHP; }
}
