using System;
using UnityEngine;

public class PlayerHealthSystem : AControllable<PlayerHealthSystem>, IEffectListener<DamageEffect>
{
    [SerializeField] float maxHP;
    [SerializeField] float currHP;
    public static event Action<float, Vector3> OnHealthPercentChange = delegate{};
    public void OnEffect(DamageEffect effect)
    {
        if (currHP <= 0) return;
        if (_currentController != _defaultController) return;

        currHP -= effect.Amount;
        Vector3 damageDir = transform.position - effect.SourcePosition;
        OnHealthPercentChange(currHP/maxHP, damageDir);
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
