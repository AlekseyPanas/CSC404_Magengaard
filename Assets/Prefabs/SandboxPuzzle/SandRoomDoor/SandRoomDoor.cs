using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SandRoomDoor : ABarrierToggle
{
    [Tooltip("An empty game object used as a door rotation point")]
    [SerializeField] private GameObject hinge;

    private bool isOpen = false;

    /** All switches must be on */
    protected override bool isInOpenConfiguration(AToggleable[] toggles) {
        return toggles.All((AToggleable tog) => tog.IsOn );
    }

    protected override void OnBarrierDisable() {
        if (isOpen) { transform.RotateAround(hinge.transform.position, Vector3.up, 90); }
        isOpen = false;
    }

    protected override void OnBarrierEnable() {
        if (!isOpen) { transform.RotateAround(hinge.transform.position, Vector3.up, -90); }
        isOpen = true;
    }
}
