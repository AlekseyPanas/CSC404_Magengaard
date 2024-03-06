using System;
using UnityEngine;

public class PlayerHealthControllerRegistrant: ControllerRegistrant {
    public Action<float, Vector3> OnHealthPercentChange = delegate{};
}


public class PlayerHealthSystem : AControllable<PlayerHealthSystem, PlayerHealthControllerRegistrant>, IEffectListener<DamageEffect>
{
    [SerializeField] float maxHP;
    [SerializeField] float currHP;
    public void OnEffect(DamageEffect effect)
    {
        if (currHP <= 0) return;
        if (_currentController != _defaultController) return;

        currHP -= effect.Amount;
        Vector3 damageDir = transform.position - effect.SourcePosition;
        _currentController.OnHealthPercentChange(currHP/maxHP, damageDir);
    }

    public void ResetHP(){
        currHP = maxHP;
    }

    protected override PlayerHealthSystem ReturnSelf() { return this; }

    void Start()
    {
        currHP = maxHP;
    }
}
