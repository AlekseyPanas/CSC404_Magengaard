using System;
using UnityEngine;

/** A Fallen page UI object listens for a pickupable, opens the UI when event received, and fires a finished inspection event when
image is closed  */
public class FallenPageUI : StaticImageUI, IInspectable
{
    private Action OnInspectEnd;

    public event Action<int, GameObject> OnUnpocketInspectableEvent;

    public void OnInspectStart(Action OnInspectEnd) {
        isOpen = true;
        this.OnInspectEnd = OnInspectEnd;
    }

    override protected void UpdateBody() {
        if (isOpen && Input.GetKeyDown(KeyCode.X)) {  // TODO REPLACE WITH GESTURE SYSTEM
            isOpen = false;
            OnInspectEnd();
        } 
    }
}