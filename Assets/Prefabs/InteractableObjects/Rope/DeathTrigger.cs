using System;
using UnityEngine;

public class DeathTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            IEffectListener<ImpactEffect>.SendEffect(other.gameObject, new ImpactEffect
            {
                Amount = 1000,
                Direction = (other.transform.position - transform.position).normalized
            });
        }
    }
}
