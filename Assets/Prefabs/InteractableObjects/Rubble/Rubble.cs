using System.Collections;
using System.Collections.Generic;
using FMODUnity;
using UnityEngine;

public class Rubble : MonoBehaviour
{
    private static readonly int ANIM_COLLAPSE = Animator.StringToHash("CollapseRubble");

    [SerializeField] private Animator _anim;
    private StudioEventEmitter _emitter;

    // Start is called before the first frame update
    void Start() {
        _emitter = GetComponent<StudioEventEmitter>();

        PickupSystem.OnTogglePickupSequence += OnTogglePickupSequence;
    }

    void OnTogglePickupSequence(bool b) {
        if (!b && !_anim.GetBool(ANIM_COLLAPSE)) { 
            _anim.SetBool(ANIM_COLLAPSE, true);
            GetComponent<BoxCollider>().enabled = false;
            _emitter.Play();
        }
    }

    void OnDestroy() {
        PickupSystem.OnTogglePickupSequence -= OnTogglePickupSequence;
    }

    // Update is called once per frame
    void Update() {
        
    }
}
