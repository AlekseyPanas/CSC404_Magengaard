using UnityEngine;

public class CutRope : MonoBehaviour, IEffectListener<ImpactEffect>
{
    private float health = 15.0f;
    private bool dead;

    public HingeJoint joint;

    private void Death()
    {
        dead = true;

        if (joint != null)
        {
            Destroy(joint);
            joint = null;
        }
    }
    
    public void OnEffect(ImpactEffect effect)
    {
        if (dead)
        {
            return;
        }
        
        health -= effect.Amount;

        if (health <= 0)
        {
            Death();
        }
    }
}
