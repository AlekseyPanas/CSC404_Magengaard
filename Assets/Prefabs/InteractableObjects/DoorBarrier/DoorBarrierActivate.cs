using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorBarrierActivate : ABarrierActivatable
{
    [SerializeField] private Animator anim;
    protected override void BarrierDisable()
    {
        
    }

    protected override void BarrierEnable()
    {
        anim.SetTrigger("Open");
    }
}
