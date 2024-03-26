using System;
using UnityEngine;

public class ProtectorElementReactionManager : AElementReactionManager
{
    [SerializeField] private float damageAngleThreshold;

    public override void OnEffect(ImpactEffect effect)
    {
        Vector3 dir = transform.position - effect.SourcePosition;
        dir = new Vector3(dir.x, 0, dir.z).normalized;
        float angle = Vector3.Angle(dir, transform.forward);
        Debug.Log(angle);
        if (angle < damageAngleThreshold){
            _aEnemy.TakeDamageWithElement(effect.Amount, Element.impact);
        } else {
            //sparks
        }
    }
}