using System.Collections.Generic;
using UnityEngine;

public class AnimationGroup : MonoBehaviour
{
    public List<Animator> animators;

    public int conditionCount = 1;
    
    private bool _moving;
    public string animationVariable;

    void SetMoving(bool value)
    {
        if (value == _moving)
        {
            return;
        }

        _moving = value;
        
        animators.ForEach(animator => animator.SetBool(animationVariable, _moving));
    }
    
    public void ConditionMet()
    {
        conditionCount--;

        if (conditionCount <= 0)
        {
            SetMoving(true);
        }
    }

    public void ConditionLost()
    {
        conditionCount++;

        if (conditionCount > 0)
        {
            SetMoving(false);
        }
    }
}