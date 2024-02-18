using System;
using UnityEngine;
using UnityEngine.Events;

public class Windmill : MonoBehaviour, IEffectListener<WindEffect>
{
    public Animator animator;
    public UnityEvent activate;
    
    private float _spinTime;
    private bool _active;
    
    private static readonly int Fast = Animator.StringToHash("Fast");
    private static readonly int Activated = Animator.StringToHash("Activated");

    public void OnEffect(WindEffect effect)
    {
        if (!_active)
        {
            activate.Invoke();
        }
        
        _spinTime = 4.0f;
        _active = true;
        
        animator.SetBool(Activated, true);
        animator.SetBool(Fast, false);
    }

    private void Update()
    {
        _spinTime -= Time.deltaTime;
        
        animator.SetBool(Fast, _spinTime < 0);
    }
}