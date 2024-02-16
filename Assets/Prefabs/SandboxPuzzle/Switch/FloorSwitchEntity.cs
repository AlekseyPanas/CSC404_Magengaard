using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloorSwitchEntity : AToggleable
{
    [Tooltip("The local/relative height (to object root) of the switch when unpressed")]
    [SerializeField] private float unpressedLocalHeight;

    [Tooltip("The local/relative height (to object root) of the switch when pressed")]
    [SerializeField] private float pressedLocalHeight;

    [Tooltip("The lerp coefficient when ")]
    [SerializeField] private float switchPressLerpVal = 0.05f;

    [Tooltip("Mass needed on the switch for it to press")]
    [SerializeField] private float massThreshold = 1f;

    [Tooltip("Do not touch this, the must have the actual switch-top injected for controlling pressing visuals")]
    [SerializeField] private GameObject switchTop;

    private float totalMass = 0;
    private float curTargetHeight;

    // Start is called before the first frame update
    void Start() {
        curTargetHeight = unpressedLocalHeight;
    }

    // Update is called once per frame
    void Update() {
        float curHeight = Mathf.Lerp(switchTop.transform.localPosition.y, curTargetHeight, switchPressLerpVal);
        switchTop.transform.localPosition = new Vector3(switchTop.transform.localPosition.x, curHeight, switchTop.transform.localPosition.z);
    }

    void OnTriggerEnter(Collider col) {
        if (col.attachedRigidbody != null) {
            totalMass += col.attachedRigidbody.mass;
            setToggleByMass();
        } else if (col.gameObject.tag == "Player") {
            totalMass += 1f;
            setToggleByMass();
        }
    }

    void OnTriggerExit(Collider col) {
        if (col.attachedRigidbody != null) {
            totalMass -= col.attachedRigidbody.mass;
            setToggleByMass();
        } else if (col.gameObject.tag == "Player") {
            totalMass -= 1f;
            setToggleByMass();
        }
    }

    /** Toggles switch if enough mass is on it */
    void setToggleByMass() {
        if (totalMass >= massThreshold) {
            setToggle(true);
            curTargetHeight = pressedLocalHeight;
        } else {
            setToggle(false);
            curTargetHeight = unpressedLocalHeight;
        }
    }
}
