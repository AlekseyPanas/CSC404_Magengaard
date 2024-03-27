using System;
using UnityEngine;

public class ProtectorElementReactionManager : AElementReactionManager
{
    [SerializeField] private float damageAngleThreshold;

    public override void OnEffect(ImpactEffect effect)
    {
        Vector3 dir = new Vector3(effect.Direction.x, 0, effect.Direction.z).normalized;
        float angle = Vector3.Angle(dir, transform.forward);
        if (angle < damageAngleThreshold){
            _aEnemy.TakeDamageWithElement(effect.Amount, Element.impact);
        }
    }

    protected override void KnockBack(Vector3 dir){
        _aEnemy.GetAgent().enabled = false;
        rb.AddForce(dir, ForceMode.Impulse);
        Invoke(nameof(ResetKnockBack), _kbDuration);
    }
}