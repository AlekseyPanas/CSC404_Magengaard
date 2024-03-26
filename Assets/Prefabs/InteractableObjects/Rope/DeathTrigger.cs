using System;
using UnityEngine;

public class DeathTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            IEffectListener<DamageEffect>.SendEffect(other.gameObject, new DamageEffect
            {
                Amount = 1000,
                SourcePosition = transform.position
            });
        }
    }
}