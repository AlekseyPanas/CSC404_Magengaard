using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

/** "Barrier" which disables when the injected list of toggleables are toggled in a defined configuration. See isInOpenConfiguration method for details */
public abstract class ABarrierToggle: NetworkBehaviour {

    [SerializeField] private AToggleable[] toggles;

    // Enables or Disables the barrier on any toggle change based on the new toggle config
    void Start() {
        foreach (AToggleable t in toggles) {
            t.changedToggleEvent += (bool toggle) => {
                if (isInOpenConfiguration(toggles)) {
                    OnBarrierDisable();
                } else { OnBarrierEnable(); } ;
            };
        }
    }

    /** Called each time one of the registered AToggleables changes state. Should return if the toggles are in a valid
    open position. Customize this as you want. (e.g, you may want 5 switches which need to be set in a correct password
    config in order for the door to open. Define that in this function) */
    protected abstract bool isInOpenConfiguration(AToggleable[] toggles);
    /** Called by abstract class when the barrier should be enabled (i.e closed) */
    protected abstract void OnBarrierEnable();
    /** Called by abstract class when the barrier should be disabled (i.e open) */
    protected abstract void OnBarrierDisable();
}
