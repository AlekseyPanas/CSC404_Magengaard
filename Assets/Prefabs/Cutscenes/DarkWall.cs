using System;
using UnityEngine;

public class DarkWall : MonoBehaviour
{
    private Animator _animator;
    private static readonly int Fade = Animator.StringToHash("Fade");

    private bool _hidden;

    private void Start()
    {
        _animator = GetComponent<Animator>();
    }

    public void DisableSelf()
    {
        gameObject.SetActive(false);
    }

    public void Hide()
    {
        if (_hidden)
        {
            return;
        }

        _hidden = true;
        
        _animator.SetBool(Fade, true);
        
        Invoke(nameof(DisableSelf), 1.0f);
    }
}