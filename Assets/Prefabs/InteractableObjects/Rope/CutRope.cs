using System;
using UnityEngine;

public class CutRope : MonoBehaviour, IEffectListener<TemperatureEffect>, IEffectListener<WindEffect>
{
    private float health = 4.0f;
    private bool dead;

    public bool x;

    public HingeJoint joint;
    public Rigidbody kinematicTrigger;

    private void Death()
    {
        dead = true;

        if (joint != null)
        {
            kinematicTrigger.isKinematic = false;
            Destroy(joint);
            joint = null;
        }
    }

    public void OnEffect(WindEffect effect)
    {
        // Jostle.
        var rigidbody = joint.GetComponent<Rigidbody>();
        
        rigidbody.AddForce(Vector3.one * 50);
    }

    public void OnEffect(TemperatureEffect effect)
    {
        if (dead)
        {
            return;
        }
        
        health -= effect.TempDelta;

        if (health <= 0)
        {
            Death();
        }
    }

    private void Update()
    {
        if (x && !dead)
        {
            Death();
        }
    }
}
