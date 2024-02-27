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

    private Action OnInspectEnd;

    public event Action<int, GameObject> OnUnpocketInspectableEvent = delegate { };

    public void OnInspectStart(Action OnInspectEnd) {
        isOpen = true;
        this.OnInspectEnd = OnInspectEnd;
        
        InputSystem.onAnyButtonPress.CallOnce(_ => {
            isOpen = false;
            OnInspectEnd();
            PagePickedUpEvent(GetComponent<Image>().sprite);
        });
    }

    protected override void UpdateBody() { }
}