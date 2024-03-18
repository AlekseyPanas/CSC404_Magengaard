using System;
using FMODUnity;
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

    [SerializeField] private EventReference _soundOpen;
    [SerializeField] private EventReference _soundClose;

    private StudioEventEmitter _emitterOpen;
    private StudioEventEmitter _emitterClose;

    protected new void Awake() {
        base.Awake();
        PlayerSpawnedEvent.OwnPlayerSpawnedEvent += (Transform pl) => {
            _pickupSys = pl.gameObject.GetComponent<AControllable<PickupSystem, ControllerRegistrant>>();
        };

        _emitterOpen = gameObject.AddComponent<StudioEventEmitter>();
        _emitterOpen.EventReference = _soundOpen;
        _emitterClose = gameObject.AddComponent<StudioEventEmitter>();
        _emitterClose.EventReference = _soundClose;
    }

    public void OnInspectStart(ControllerRegistrant pickupRegistrant, GestureSystemControllerRegistrant gestureRegistrant) {
        isOpen = true;
        _registrant = pickupRegistrant;
        _registrant.OnInterrupt = Close;

        _emitterOpen.Play();
    }

    void Close() {
        if (isOpen) {
            isOpen = false;
            _pickupSys.DeRegisterController(_registrant);
            PagePickedUpEvent(GetComponent<Image>().sprite);
        }
    }

    protected override void UpdateBody() { }

    protected override void OnFullyOpen() {
        InputSystem.onAnyButtonPress.CallOnce(e => {
            Close();
            _emitterClose.Play();
        });
    }
}