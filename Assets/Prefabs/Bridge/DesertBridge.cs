using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DesertBridge : MonoBehaviour
{
    public Collider[] bridgeBody;
    public Collider[] bridgePit;

    public Animator animator;
    private static readonly int Extended = Animator.StringToHash("Extended");

    public void Extend()
    {
        foreach (var body in bridgeBody)
        {
            body.enabled = true;
        }

        foreach (var body in bridgePit)
        {
            body.enabled = false;
        }
        
        animator.SetBool(Extended, true);
    }
}
