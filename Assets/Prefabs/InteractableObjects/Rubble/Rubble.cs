using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rubble : MonoBehaviour
{
    private static readonly int ANIM_COLLAPSE = Animator.StringToHash("CollapseRubble");

    [SerializeField] private Animator _anim;

    // Start is called before the first frame update
    void Start() {
        PickupSystem.OnChangePuppetModeDueToInspectionEvent += (bool b) => { 
            if (!b && !_anim.GetBool(ANIM_COLLAPSE)) { 
                _anim.SetBool(ANIM_COLLAPSE, true);
                GetComponent<BoxCollider>().enabled = false;
            }
        };
    }

    // Update is called once per frame
    void Update() {
        
    }
}
