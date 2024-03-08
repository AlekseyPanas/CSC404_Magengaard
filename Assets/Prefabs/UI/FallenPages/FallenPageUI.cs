using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.UI;

/** A Fallen page UI object listens for a pickupable, opens the UI when event received, and fires a finished inspection event when
image is closed  */
public class FallenPageUI : StaticImageUI, IInspectable
{
    public static event Action<Sprite> PagePickedUpEvent = delegate { };  // Listen for this event to get page sprites

    private AControllable<PickupSystem, ControllerRegistrant> _pickupSys;
    private ControllerRegistrant _registrant;
    public event Action<int, GameObject> OnUnpocketInspectableEvent = delegate { };

    void Awake() {
        PlayerSpawnedEvent.OwnPlayerSpawnedEvent += (Transform pl) => {
            _pickupSys = pl.gameObject.GetComponent<AControllable<PickupSystem, ControllerRegistrant>>();
        };
    }

    public void OnInspectStart(ControllerRegistrant pickupRegistrant, GestureSystemControllerRegistrant gestureRegistrant) {
        isOpen = true;
        _registrant = pickupRegistrant;
        _registrant.OnInterrupt = Close;
        
        InputSystem.onAnyButtonPress.CallOnce(e => {
            if (isOpen) { _pickupSys.DeRegisterController(_registrant); }
            Close();
        });
    }

    void Close() {
        if (isOpen) {
            isOpen = false;
            PagePickedUpEvent(GetComponent<Image>().sprite);
        }
    }

    protected override void UpdateBody() { }
}