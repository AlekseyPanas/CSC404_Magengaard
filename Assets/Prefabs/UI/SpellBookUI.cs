using System;
using UnityEngine;


/** UI for the spellbook which can be re-opened  */
public class SpellBookUI : StaticImageUI, IInspectable
{
    private Action OnInspectEnd;
    private int _PickupablesListBookIndex = (int)PickupablesNetworkPrefabListIndexes.BOOK;

    public event Action<int, GameObject> OnUnpocketInspectableEvent = delegate { };

    public void OnInspectStart(Action OnInspectEnd) {
        isOpen = true;
        this.OnInspectEnd = OnInspectEnd;
    }

    override protected void UpdateBody() {
        if (isOpen && Input.GetKeyDown(KeyCode.X)) {  // TODO REPLACE WITH GESTURE SYSTEM
            isOpen = false;
            OnInspectEnd();
        } else if (!isOpen && Input.GetKeyDown(KeyCode.B)) {
            OnUnpocketInspectableEvent(_PickupablesListBookIndex, gameObject);
        }
    }
}
