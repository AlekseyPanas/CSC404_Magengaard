using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class CapsuleCore : NetworkBehaviour, IInspectable {
    public static Action<int, SpellElementColorPalette> OnGetCapsuleOfElement = delegate{};

    private AControllable<PickupSystem, ControllerRegistrant> _pickupSys;
    [SerializeField] private int _elementID;
    private SpellElementColorPalette palette;

    // TODO: Add proofing
    public void OnInspectStart(ControllerRegistrant pickupRegistrant, GestureSystemControllerRegistrant gestureRegistrant) {
        _pickupSys.DeRegisterController(pickupRegistrant);

        OnGetCapsuleOfElement(_elementID, palette);
    }

    // Start is called before the first frame update
    void Start(){
        PlayerSpawnedEvent.OwnPlayerSpawnedEvent += (Transform pl) => {
            _pickupSys = pl.gameObject.GetComponent<AControllable<PickupSystem, ControllerRegistrant>>();
        };

        // TODO: Fix
        if (_elementID == 0) {
            palette = new WindPalette();
        } else if (_elementID == 1) {
            palette = new IcePalette();
        } else {
            palette = new FirePalette();
        }
    }
}
