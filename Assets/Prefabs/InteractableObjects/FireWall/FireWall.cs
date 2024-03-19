using System;
using UnityEngine;

public class FireWall : MonoBehaviour
{
    public Animator animator;
    
    private static readonly int Burning = Animator.StringToHash("Burning");

    public void Appear()
    {
        gameObject.SetActive(true);
        
        animator.SetBool(Burning, true);
    }

    private void Disable()
    {
        gameObject.SetActive(false);
    }

    public void Disappear()
    {
        animator.SetBool(Burning, false);
        
        Invoke(nameof(Disable), 1.0f);
    }
}